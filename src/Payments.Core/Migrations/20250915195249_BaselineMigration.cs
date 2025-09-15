using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payments.Core.Migrations
{
    /// <summary>
    /// Baseline migration - Database already exists with all tables.
    /// This migration serves as a starting point for future schema changes.
    /// </summary>
    public partial class BaselineMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Database already exists with all tables
            // This is a baseline migration for existing database
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot downgrade from baseline - database existed before migrations
            throw new NotSupportedException("Cannot downgrade from baseline migration. Database existed before EF migrations were implemented.");
        }
    }
}
FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CampusKerberos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

migrationBuilder.CreateTable(
    name: "MoneyMovementJobRecords",
    columns: table => new
    {
        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        RanOn = table.Column<DateTime>(type: "datetime2", nullable: false),
        Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_MoneyMovementJobRecords", x => x.Id);
    });

migrationBuilder.CreateTable(
    name: "TaxReportJobRecords",
    columns: table => new
    {
        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        RanOn = table.Column<DateTime>(type: "datetime2", nullable: false),
        Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_TaxReportJobRecords", x => x.Id);
    });

migrationBuilder.CreateTable(
    name: "TeamRoles",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_TeamRoles", x => x.Id);
    });

migrationBuilder.CreateTable(
    name: "Teams",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
        Slug = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
        ContactName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
        ContactEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
        ContactPhoneNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
        IsActive = table.Column<bool>(type: "bit", nullable: false),
        ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
        WebHookApiKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Teams", x => x.Id);
    });

migrationBuilder.CreateTable(
    name: "AspNetRoleClaims",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
        table.ForeignKey(
            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            column: x => x.RoleId,
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "AspNetUserClaims",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
        table.ForeignKey(
            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "AspNetUserLogins",
    columns: table => new
    {
        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
        ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
        ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
        table.ForeignKey(
            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "AspNetUserRoles",
    columns: table => new
    {
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
        table.ForeignKey(
            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            column: x => x.RoleId,
            principalTable: "AspNetRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "AspNetUserTokens",
    columns: table => new
    {
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
        Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
        Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
        table.ForeignKey(
            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "Logs",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
        MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
        TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
        Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Properties = table.Column<string>(type: "xml", nullable: true),
        LogEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
        JobId = table.Column<string>(type: "nvarchar(450)", nullable: true),
        JobName = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Logs", x => x.Id);
        table.ForeignKey(
            name: "FK_Logs_MoneyMovementJobRecords_JobId",
            column: x => x.JobId,
            principalTable: "MoneyMovementJobRecords",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        table.ForeignKey(
            name: "FK_Logs_TaxReportJobRecords_JobId",
            column: x => x.JobId,
            principalTable: "TaxReportJobRecords",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    });

migrationBuilder.CreateTable(
    name: "Coupons",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
        DiscountPercent = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
        DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
        ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
        TeamId = table.Column<int>(type: "int", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Coupons", x => x.Id);
        table.ForeignKey(
            name: "FK_Coupons_Teams_TeamId",
            column: x => x.TeamId,
            principalTable: "Teams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "FinancialAccounts",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Chart = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
        Account = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
        FinancialSegmentString = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
        SubAccount = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
        Project = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
        IsDefault = table.Column<bool>(type: "bit", nullable: false),
        IsActive = table.Column<bool>(type: "bit", nullable: false),
        TeamId = table.Column<int>(type: "int", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
        table.ForeignKey(
            name: "FK_FinancialAccounts_Teams_TeamId",
            column: x => x.TeamId,
            principalTable: "Teams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "TeamPermissions",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        TeamId = table.Column<int>(type: "int", nullable: false),
        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
        RoleId = table.Column<int>(type: "int", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_TeamPermissions", x => x.Id);
        table.ForeignKey(
            name: "FK_TeamPermissions_AspNetUsers_UserId",
            column: x => x.UserId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_TeamPermissions_TeamRoles_RoleId",
            column: x => x.RoleId,
            principalTable: "TeamRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_TeamPermissions_Teams_TeamId",
            column: x => x.TeamId,
            principalTable: "Teams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "WebHooks",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        TeamId = table.Column<int>(type: "int", nullable: false),
        IsActive = table.Column<bool>(type: "bit", nullable: false),
        Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
        TriggerOnPaid = table.Column<bool>(type: "bit", nullable: false),
        TriggerOnReconcile = table.Column<bool>(type: "bit", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_WebHooks", x => x.Id);
        table.ForeignKey(
            name: "FK_WebHooks_Teams_TeamId",
            column: x => x.TeamId,
            principalTable: "Teams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "Invoices",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        LinkId = table.Column<string>(type: "nvarchar(max)", nullable: true),
        DraftCount = table.Column<int>(type: "int", nullable: false),
        CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CustomerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CustomerCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Memo = table.Column<string>(type: "nvarchar(max)", nullable: true),
        TaxPercent = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
        DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
        Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CouponId = table.Column<int>(type: "int", nullable: true),
        ManualDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        AccountId = table.Column<int>(type: "int", nullable: true),
        TeamId = table.Column<int>(type: "int", nullable: false),
        Sent = table.Column<bool>(type: "bit", nullable: false),
        SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
        Paid = table.Column<bool>(type: "bit", nullable: false),
        PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
        Refunded = table.Column<bool>(type: "bit", nullable: false),
        RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
        PaymentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
        PaymentProcessorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
        KfsTrackingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
        Deleted = table.Column<bool>(type: "bit", nullable: false),
        DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
        CalculatedSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        CalculatedDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        CalculatedTaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        CalculatedTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        CalculatedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Invoices", x => x.Id);
        table.ForeignKey(
            name: "FK_Invoices_Coupons_CouponId",
            column: x => x.CouponId,
            principalTable: "Coupons",
            principalColumn: "Id");
        table.ForeignKey(
            name: "FK_Invoices_FinancialAccounts_AccountId",
            column: x => x.AccountId,
            principalTable: "FinancialAccounts",
            principalColumn: "Id");
        table.ForeignKey(
            name: "FK_Invoices_Teams_TeamId",
            column: x => x.TeamId,
            principalTable: "Teams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    });

migrationBuilder.CreateTable(
    name: "History",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        InvoiceId = table.Column<int>(type: "int", nullable: false),
        Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
        ActionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
        Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Actor = table.Column<string>(type: "nvarchar(max)", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_History", x => x.Id);
        table.ForeignKey(
            name: "FK_History_Invoices_InvoiceId",
            column: x => x.InvoiceId,
            principalTable: "Invoices",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "InvoiceAttachments",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
        FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Size = table.Column<long>(type: "bigint", nullable: false),
        InvoiceId = table.Column<int>(type: "int", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_InvoiceAttachments", x => x.Id);
        table.ForeignKey(
            name: "FK_InvoiceAttachments_Invoices_InvoiceId",
            column: x => x.InvoiceId,
            principalTable: "Invoices",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });

migrationBuilder.CreateTable(
    name: "InvoiceLinks",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        LinkId = table.Column<string>(type: "nvarchar(max)", nullable: true),
        InvoiceId = table.Column<int>(type: "int", nullable: true),
        Expired = table.Column<bool>(type: "bit", nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_InvoiceLinks", x => x.Id);
        table.ForeignKey(
            name: "FK_InvoiceLinks_Invoices_InvoiceId",
            column: x => x.InvoiceId,
            principalTable: "Invoices",
            principalColumn: "Id");
    });

migrationBuilder.CreateTable(
    name: "LineItems",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        TaxExempt = table.Column<bool>(type: "bit", nullable: false),
        InvoiceId = table.Column<int>(type: "int", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_LineItems", x => x.Id);
        table.ForeignKey(
            name: "FK_LineItems_Invoices_InvoiceId",
            column: x => x.InvoiceId,
            principalTable: "Invoices",
            principalColumn: "Id");
    });

migrationBuilder.CreateTable(
    name: "PaymentEvents",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Processor = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ProcessorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Decision = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        BillingFirstName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
        BillingLastName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
        BillingEmail = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
        BillingCompany = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
        BillingPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
        BillingStreet1 = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        BillingStreet2 = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
        BillingCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
        BillingState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
        BillingCountry = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
        BillingPostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
        CardType = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
        CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
        CardExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
        ReturnedResults = table.Column<string>(type: "nvarchar(max)", nullable: true),
        OccuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
        InvoiceId = table.Column<int>(type: "int", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_PaymentEvents", x => x.Id);
        table.ForeignKey(
            name: "FK_PaymentEvents_Invoices_InvoiceId",
            column: x => x.InvoiceId,
            principalTable: "Invoices",
            principalColumn: "Id");
    });

migrationBuilder.CreateIndex(
    name: "IX_AspNetRoleClaims_RoleId",
    table: "AspNetRoleClaims",
    column: "RoleId");

migrationBuilder.CreateIndex(
    name: "RoleNameIndex",
    table: "AspNetRoles",
    column: "NormalizedName",
    unique: true,
    filter: "[NormalizedName] IS NOT NULL");

migrationBuilder.CreateIndex(
    name: "IX_AspNetUserClaims_UserId",
    table: "AspNetUserClaims",
    column: "UserId");

migrationBuilder.CreateIndex(
    name: "IX_AspNetUserLogins_UserId",
    table: "AspNetUserLogins",
    column: "UserId");

migrationBuilder.CreateIndex(
    name: "IX_AspNetUserRoles_RoleId",
    table: "AspNetUserRoles",
    column: "RoleId");

migrationBuilder.CreateIndex(
    name: "EmailIndex",
    table: "AspNetUsers",
    column: "NormalizedEmail");

migrationBuilder.CreateIndex(
    name: "UserNameIndex",
    table: "AspNetUsers",
    column: "NormalizedUserName",
    unique: true,
    filter: "[NormalizedUserName] IS NOT NULL");

migrationBuilder.CreateIndex(
    name: "IX_Coupons_TeamId",
    table: "Coupons",
    column: "TeamId");

migrationBuilder.CreateIndex(
    name: "IX_FinancialAccounts_TeamId",
    table: "FinancialAccounts",
    column: "TeamId");

migrationBuilder.CreateIndex(
    name: "IX_History_InvoiceId",
    table: "History",
    column: "InvoiceId");

migrationBuilder.CreateIndex(
    name: "IX_InvoiceAttachments_InvoiceId",
    table: "InvoiceAttachments",
    column: "InvoiceId");

migrationBuilder.CreateIndex(
    name: "IX_InvoiceLinks_InvoiceId",
    table: "InvoiceLinks",
    column: "InvoiceId");

migrationBuilder.CreateIndex(
    name: "IX_Invoices_AccountId",
    table: "Invoices",
    column: "AccountId");

migrationBuilder.CreateIndex(
    name: "IX_Invoices_CouponId",
    table: "Invoices",
    column: "CouponId");

migrationBuilder.CreateIndex(
    name: "IX_Invoices_TeamId",
    table: "Invoices",
    column: "TeamId");

migrationBuilder.CreateIndex(
    name: "IX_LineItems_InvoiceId",
    table: "LineItems",
    column: "InvoiceId");

migrationBuilder.CreateIndex(
    name: "IX_Logs_JobId",
    table: "Logs",
    column: "JobId");

migrationBuilder.CreateIndex(
    name: "IX_PaymentEvents_InvoiceId",
    table: "PaymentEvents",
    column: "InvoiceId");

migrationBuilder.CreateIndex(
    name: "IX_TeamPermissions_RoleId",
    table: "TeamPermissions",
    column: "RoleId");

migrationBuilder.CreateIndex(
    name: "IX_TeamPermissions_TeamId",
    table: "TeamPermissions",
    column: "TeamId");

migrationBuilder.CreateIndex(
    name: "IX_TeamPermissions_UserId",
    table: "TeamPermissions",
    column: "UserId");

migrationBuilder.CreateIndex(
    name: "IX_WebHooks_TeamId",
    table: "WebHooks",
    column: "TeamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(
        name: "AspNetRoleClaims");

    migrationBuilder.DropTable(
        name: "AspNetUserClaims");

    migrationBuilder.DropTable(
        name: "AspNetUserLogins");

    migrationBuilder.DropTable(
        name: "AspNetUserRoles");

    migrationBuilder.DropTable(
        name: "AspNetUserTokens");

    migrationBuilder.DropTable(
        name: "History");

    migrationBuilder.DropTable(
        name: "InvoiceAttachments");

    migrationBuilder.DropTable(
        name: "InvoiceLinks");

    migrationBuilder.DropTable(
        name: "LineItems");

    migrationBuilder.DropTable(
        name: "Logs");

    migrationBuilder.DropTable(
        name: "PaymentEvents");

    migrationBuilder.DropTable(
        name: "TeamPermissions");

    migrationBuilder.DropTable(
        name: "WebHooks");

    migrationBuilder.DropTable(
        name: "AspNetRoles");

    migrationBuilder.DropTable(
        name: "MoneyMovementJobRecords");

    migrationBuilder.DropTable(
        name: "TaxReportJobRecords");

    migrationBuilder.DropTable(
        name: "Invoices");

    migrationBuilder.DropTable(
        name: "AspNetUsers");

    migrationBuilder.DropTable(
        name: "TeamRoles");

    migrationBuilder.DropTable(
        name: "Coupons");

    migrationBuilder.DropTable(
        name: "FinancialAccounts");

    migrationBuilder.DropTable(
        name: "Teams");
}
    }
}
