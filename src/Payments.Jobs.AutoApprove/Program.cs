using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mjml.AspNetCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Jobs;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Core.Models.Invoice;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Jobs.Core;
using Serilog;
using System;


namespace Payments.Jobs.MoneyMovement
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static void Main(string[] args)
        {
            // setup env
            Configure();


            _log = Log.Logger
                .ForContext("jobname", "AutoApprove");

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);
            _log.Information("AutoApprove Version 1");

            // setup di
            var provider = ConfigureServices();
            var dbContext = provider.GetService<ApplicationDbContext>();
            var emailService = provider.GetService<IEmailService>();


            try
            {
                if(dbContext == null)
                {
                    throw new InvalidOperationException("Failed to obtain ApplicationDbContext from service provider.");
                }
                if(emailService == null)
                {
                    throw new InvalidOperationException("Failed to obtain IEmailService from service provider.");
                }

                var financeSettings = provider.GetService<IOptions<FinanceSettings>>()?.Value;
                if (financeSettings == null)
                {
                    throw new InvalidOperationException("FinanceSettings configuration is missing or invalid.");
                }
                var daysToAutoApprove = financeSettings.RechargeAutoApproveDays;
                var dateThreshold = DateTime.UtcNow.AddDays(-daysToAutoApprove);

                var invoices =  dbContext.Invoices
                    .Include(i => i.RechargeAccounts)
                    .Include(i => i.Team)
                    .Where(a => a.Type == Invoice.InvoiceTypes.Recharge && a.Status == Invoice.StatusCodes.PendingApproval &&
                        a.PaidAt != null && a.PaidAt <= dateThreshold).ToList();
                _log.Information("Found {count} invoices to auto-approve", invoices.Count);
                foreach (var invoice in invoices)
                {
                    invoice.Status = Invoice.StatusCodes.Approved;
                    foreach (var ra in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit && string.IsNullOrWhiteSpace( a.ApprovedByKerb)))
                    {
                        ra.ApprovedByKerb = "Automated";
                        ra.ApprovedByName = "System";                       
                    }

                    invoice.Paid = true;

                    var approvalAction = new History()
                    {
                        Type = HistoryActionTypes.RechargeApprovedByFinancialApprover.TypeCode,
                        ActionDateTime = DateTime.UtcNow,
                        Actor = "System",
                        Data = "All debit recharge accounts have been approved."
                    };

                    invoice.History.Add(approvalAction);
                    dbContext.Invoices.Update(invoice);
                    _log.Information("Auto-approved invoice {invoiceId}", invoice.Id);

                    try
                    {
                        SendApprovalModel people = new SendApprovalModel();
                        var approvers = new List<EmailRecipient>();
                        foreach (var ra in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
                        {
                            if (ra.ApprovedByKerb == null || ra.ApprovedByKerb == "System")
                            {
                                continue;
                            }
                            var user =  dbContext.Users.FirstOrDefault(u => u.CampusKerberos == ra.ApprovedByKerb);
                            if ((user != null && user.Email != null))
                            {
                                approvers.Add(new EmailRecipient()
                                {
                                    Email = user.Email,
                                    Name = ra.ApprovedByName
                                });
                            }
                        }

                        people.emails = approvers.DistinctBy(a => a.Email.ToLower()).ToArray();
                        emailService.SendRechargeReceipt(invoice, people).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Failed to send auto-approval receipt email for invoice {invoiceId}", invoice.Id, ex);
                    }
                }
            }
            catch (Exception ex)
            {

                _log.Error("Error running AutoApprove job", ex);
                throw;
            }

            _log.Information("AutoApprove job completed");

        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<FinanceSettings>(Configuration.GetSection("Finance"));
            services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));


            // db service
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddTransient<IEmailService, SparkpostEmailService>();
            services.AddMjmlServices();


            return services.BuildServiceProvider();
        }
    }
}
