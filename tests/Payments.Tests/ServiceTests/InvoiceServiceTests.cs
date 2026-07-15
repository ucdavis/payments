using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Invoice;
using Payments.Core.Models.Validation;
using Payments.Core.Resources;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Mvc.Services;
using TestHelpers.Helpers;
using Xunit;

namespace payments.Tests.ServiceTests
{
    public class InvoiceServiceTests
    {
        [Fact]
        public async Task CreateInvoices_CopiesExternalDetailsToCreatedInvoices()
        {
            var team = new Team { Id = 1 };
            var account = new FinancialAccount
            {
                Id = 2,
                Team = team,
                IsActive = true,
            };
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.FinancialAccounts)
                .Returns(new List<FinancialAccount> { account }.AsQueryable().MockAsyncDbSet().Object);
            dbContext
                .Setup(context => context.Invoices)
                .Returns(new Mock<DbSet<Invoice>>().Object);

            var service = new InvoiceService(
                dbContext.Object,
                new Mock<IEmailService>().Object,
                new Mock<IAggieEnterpriseService>().Object);
            var model = new CreateInvoiceModel
            {
                AccountId = account.Id,
                Customers = new List<CreateInvoiceCustomerModel>
                {
                    new CreateInvoiceCustomerModel { Email = "customer@example.com" },
                },
                Items = new List<CreateInvoiceItemModel>
                {
                    new CreateInvoiceItemModel { Description = "Item", Quantity = 1, Amount = 10 },
                },
                Attachments = new List<CreateInvoiceAttachmentModel>(),
                ExternalIdentifier = "External System",
                ExternalId = "record-123",
                ExternalLink = "https://example.com/records/record-123",
            };

            var invoices = await service.CreateInvoices(model, team);

            var invoice = Assert.Single(invoices);
            Assert.Equal(model.ExternalIdentifier, invoice.ExternalIdentifier);
            Assert.Equal(model.ExternalId, invoice.ExternalId);
            Assert.Equal(model.ExternalLink, invoice.ExternalLink);
        }

        [Fact]
        public async Task CreateInvoices_InvalidRechargeAccount_IdentifiesRechargeAccountsParameter()
        {
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.FinancialAccounts)
                .Returns(new List<FinancialAccount>().AsQueryable().MockAsyncDbSet().Object);

            var aggieEnterpriseService = new Mock<IAggieEnterpriseService>();
            aggieEnterpriseService
                .Setup(service => service.IsRechargeAccountValid(
                    "invalid-chart-string", RechargeAccount.CreditDebit.Credit, true))
                .ReturnsAsync(new AccountValidationModel { IsValid = false });

            var service = new InvoiceService(
                dbContext.Object,
                new Mock<IEmailService>().Object,
                aggieEnterpriseService.Object);
            var model = new CreateInvoiceModel
            {
                Type = Invoice.InvoiceTypes.Recharge,
                RechargeAccounts = new List<RechargeAccount>
                {
                    new RechargeAccount
                    {
                        Direction = RechargeAccount.CreditDebit.Credit,
                        FinancialSegmentString = "invalid-chart-string",
                        Amount = 1,
                    },
                },
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateInvoices(model, new Team { Id = 1 }));

            Assert.Equal(nameof(model.RechargeAccounts), exception.ParamName);
        }

        [Fact]
        public async Task SendInvoice_ZeroCreditCardInvoice_MarksInvoiceCompleted()
        {
            var invoice = new Invoice
            {
                Status = Invoice.StatusCodes.Draft,
                Type = Invoice.InvoiceTypes.CreditCard,
                ManualDiscount = 10,
                Items = new List<LineItem>
                {
                    new LineItem { Total = 10 },
                },
            };
            invoice.UpdateCalculatedValues();

            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.InvoiceLinks)
                .Returns(new Mock<DbSet<InvoiceLink>>().Object);
            var emailService = new Mock<IEmailService>();
            var service = new InvoiceService(
                dbContext.Object,
                emailService.Object,
                new Mock<IAggieEnterpriseService>().Object);

            await service.SendInvoice(invoice, new SendInvoiceModel());

            Assert.Equal(Invoice.StatusCodes.Completed, invoice.Status);
            Assert.True(invoice.Sent);
            Assert.True(invoice.Paid);
            Assert.NotNull(invoice.PaidAt);
            Assert.Equal(PaymentTypes.Manual, invoice.PaymentType);
            Assert.Contains(invoice.History, history => history.Type == "mark-paid");
            emailService.Verify(
                service => service.SendInvoice(invoice, null, null),
                Times.Once);
        }

        [Fact]
        public async Task SendInvoice_CouponZeroedCreditCardInvoice_PreservesCouponDiscount()
        {
            var invoice = new Invoice
            {
                Status = Invoice.StatusCodes.Draft,
                Type = Invoice.InvoiceTypes.CreditCard,
                Coupon = new Coupon { DiscountAmount = 10 },
                Items = new List<LineItem>
                {
                    new LineItem { Total = 10 },
                },
            };
            invoice.UpdateCalculatedValues();

            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.InvoiceLinks)
                .Returns(new Mock<DbSet<InvoiceLink>>().Object);
            var service = new InvoiceService(
                dbContext.Object,
                new Mock<IEmailService>().Object,
                new Mock<IAggieEnterpriseService>().Object);

            await service.SendInvoice(invoice, new SendInvoiceModel());
            invoice.UpdateCalculatedValues();

            Assert.Equal(Invoice.StatusCodes.Completed, invoice.Status);
            Assert.True(invoice.Paid);
            Assert.Equal(10, invoice.ManualDiscount);
            Assert.Equal(0, invoice.CalculatedTotal);
            Assert.Equal(PaymentTypes.Coupon, invoice.PaymentType);
        }

        [Fact]
        public async Task SendFinancialApprovalRejected_SendsCustomerAndDistinctTeamEditorsAndAdmins()
        {
            var team = new Team { Id = 7, Name = "Test Team", Slug = "test-team" };
            var invoice = new Invoice
            {
                Id = 42,
                Team = team,
                CustomerEmail = "customer@example.com",
                CustomerName = "Customer",
            };
            var approver = new User
            {
                Id = "approver",
                Email = "approver@example.com",
                Name = "Financial Approver",
            };
            var editorRole = new TeamRole { Name = TeamRole.Codes.Editor };
            var permissions = new List<TeamPermission>
            {
                new TeamPermission
                {
                    TeamId = team.Id,
                    Role = editorRole,
                    User = new User { Id = "editor-1", Email = "editor1@example.com", Name = "Editor One" },
                },
                new TeamPermission
                {
                    TeamId = team.Id,
                    Role = new TeamRole { Name = TeamRole.Codes.Admin },
                    User = new User { Id = "duplicate-editor", Email = "EDITOR1@example.com", Name = "Duplicate Editor" },
                },
                new TeamPermission
                {
                    TeamId = team.Id,
                    Role = editorRole,
                    User = new User { Id = "editor-2", Email = "editor2@example.com", Name = "Editor Two" },
                },
                new TeamPermission
                {
                    TeamId = team.Id,
                    Role = new TeamRole { Name = TeamRole.Codes.Admin },
                    User = new User { Id = "admin", Email = "admin@example.com", Name = "Admin" },
                },
                new TeamPermission
                {
                    TeamId = 99,
                    Role = editorRole,
                    User = new User { Id = "other-team", Email = "other@example.com", Name = "Other Team Editor" },
                },
            };
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.TeamPermissions)
                .Returns(permissions.AsQueryable().MockAsyncDbSet().Object);
            var emailService = new Mock<IEmailService>();
            var service = new InvoiceService(
                dbContext.Object,
                emailService.Object,
                new Mock<IAggieEnterpriseService>().Object);

            await service.SendFinancialApprovalRejected(invoice, "Incorrect account", approver);

            emailService.Verify(
                email => email.SendFinancialApprovalRejectedCustomer(invoice, "Incorrect account", approver),
                Times.Once);
            emailService.Verify(
                email => email.SendFinancialApprovalRejectedEditors(
                    invoice,
                    "Incorrect account",
                    It.Is<IReadOnlyCollection<User>>(editors =>
                        editors.Count == 3 &&
                        editors.Any(editor => editor.Email == "editor1@example.com") &&
                        editors.Any(editor => editor.Email == "editor2@example.com") &&
                        editors.Any(editor => editor.Email == "admin@example.com"))),
                Times.Once);
        }

        [Fact]
        public async Task SendFinancialApprovalRejected_CustomerFailureStillAttemptsEditorsAndDoesNotThrow()
        {
            var team = new Team { Id = 7 };
            var invoice = new Invoice { Id = 42, Team = team };
            var approver = new User { Email = "approver@example.com", Name = "Financial Approver" };
            var editor = new User { Email = "editor@example.com", Name = "Editor" };
            var permissions = new List<TeamPermission>
            {
                new TeamPermission
                {
                    TeamId = team.Id,
                    Role = new TeamRole { Name = TeamRole.Codes.Editor },
                    User = editor,
                },
            };
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            dbContext
                .Setup(context => context.TeamPermissions)
                .Returns(permissions.AsQueryable().MockAsyncDbSet().Object);
            var emailService = new Mock<IEmailService>();
            emailService
                .Setup(email => email.SendFinancialApprovalRejectedCustomer(invoice, "Incorrect account", approver))
                .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
            emailService
                .Setup(email => email.SendFinancialApprovalRejectedEditors(
                    invoice,
                    "Incorrect account",
                    It.IsAny<IReadOnlyCollection<User>>()))
                .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
            var service = new InvoiceService(
                dbContext.Object,
                emailService.Object,
                new Mock<IAggieEnterpriseService>().Object);

            await service.SendFinancialApprovalRejected(invoice, "Incorrect account", approver);

            emailService.Verify(
                email => email.SendFinancialApprovalRejectedEditors(
                    invoice,
                    "Incorrect account",
                    It.Is<IReadOnlyCollection<User>>(editors => editors.Count == 1 && editors.Single() == editor)),
                Times.Once);
        }
    }
}
