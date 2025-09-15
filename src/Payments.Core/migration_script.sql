IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(50) NULL,
        [LastName] nvarchar(50) NULL,
        [Name] nvarchar(256) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [CampusKerberos] nvarchar(50) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [MoneyMovementJobRecords] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NULL,
        [RanOn] datetime2 NOT NULL,
        [Status] nvarchar(max) NULL,
        CONSTRAINT [PK_MoneyMovementJobRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [TaxReportJobRecords] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(max) NULL,
        [RanOn] datetime2 NOT NULL,
        [Status] nvarchar(max) NULL,
        CONSTRAINT [PK_TaxReportJobRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [TeamRoles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_TeamRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [Teams] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(128) NOT NULL,
        [Slug] nvarchar(40) NOT NULL,
        [ContactName] nvarchar(128) NULL,
        [ContactEmail] nvarchar(128) NULL,
        [ContactPhoneNumber] nvarchar(40) NULL,
        [IsActive] bit NOT NULL,
        [ApiKey] nvarchar(max) NULL,
        [WebHookApiKey] nvarchar(128) NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [Logs] (
        [Id] int NOT NULL IDENTITY,
        [Source] nvarchar(max) NULL,
        [Message] nvarchar(max) NULL,
        [MessageTemplate] nvarchar(max) NULL,
        [Level] nvarchar(max) NULL,
        [TimeStamp] datetimeoffset NOT NULL,
        [Exception] nvarchar(max) NULL,
        [Properties] xml NULL,
        [LogEvent] nvarchar(max) NULL,
        [CorrelationId] nvarchar(max) NULL,
        [JobId] nvarchar(450) NULL,
        [JobName] nvarchar(max) NULL,
        CONSTRAINT [PK_Logs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Logs_MoneyMovementJobRecords_JobId] FOREIGN KEY ([JobId]) REFERENCES [MoneyMovementJobRecords] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Logs_TaxReportJobRecords_JobId] FOREIGN KEY ([JobId]) REFERENCES [TaxReportJobRecords] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [Coupons] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [Code] nvarchar(max) NULL,
        [DiscountPercent] decimal(18,5) NULL,
        [DiscountAmount] decimal(18,2) NULL,
        [ExpiresAt] datetime2 NULL,
        [TeamId] int NOT NULL,
        CONSTRAINT [PK_Coupons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Coupons_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [FinancialAccounts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(128) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Chart] nvarchar(1) NULL,
        [Account] nvarchar(7) NULL,
        [FinancialSegmentString] nvarchar(128) NULL,
        [SubAccount] nvarchar(5) NULL,
        [Project] nvarchar(9) NULL,
        [IsDefault] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [TeamId] int NOT NULL,
        CONSTRAINT [PK_FinancialAccounts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FinancialAccounts_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [TeamPermissions] (
        [Id] int NOT NULL IDENTITY,
        [TeamId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_TeamPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeamPermissions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeamPermissions_TeamRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [TeamRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeamPermissions_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [WebHooks] (
        [Id] int NOT NULL IDENTITY,
        [TeamId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [Url] nvarchar(max) NULL,
        [ContentType] nvarchar(max) NULL,
        [TriggerOnPaid] bit NOT NULL,
        [TriggerOnReconcile] bit NOT NULL,
        CONSTRAINT [PK_WebHooks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WebHooks_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [Invoices] (
        [Id] int NOT NULL IDENTITY,
        [LinkId] nvarchar(max) NULL,
        [DraftCount] int NOT NULL,
        [CustomerName] nvarchar(max) NULL,
        [CustomerAddress] nvarchar(max) NULL,
        [CustomerEmail] nvarchar(max) NULL,
        [CustomerCompany] nvarchar(max) NULL,
        [Memo] nvarchar(max) NULL,
        [TaxPercent] decimal(18,5) NOT NULL,
        [DueDate] datetime2 NULL,
        [Status] nvarchar(max) NULL,
        [CouponId] int NULL,
        [ManualDiscount] decimal(18,2) NOT NULL,
        [AccountId] int NULL,
        [TeamId] int NOT NULL,
        [Sent] bit NOT NULL,
        [SentAt] datetime2 NULL,
        [Paid] bit NOT NULL,
        [PaidAt] datetime2 NULL,
        [Refunded] bit NOT NULL,
        [RefundedAt] datetime2 NULL,
        [PaymentType] nvarchar(max) NULL,
        [PaymentProcessorId] nvarchar(max) NULL,
        [KfsTrackingNumber] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Deleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        [CalculatedSubtotal] decimal(18,2) NOT NULL,
        [CalculatedDiscount] decimal(18,2) NOT NULL,
        [CalculatedTaxableAmount] decimal(18,2) NOT NULL,
        [CalculatedTaxAmount] decimal(18,2) NOT NULL,
        [CalculatedTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Invoices_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [Coupons] ([Id]),
        CONSTRAINT [FK_Invoices_FinancialAccounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [FinancialAccounts] ([Id]),
        CONSTRAINT [FK_Invoices_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [History] (
        [Id] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [ActionDateTime] datetime2 NOT NULL,
        [Data] nvarchar(max) NULL,
        [Actor] nvarchar(max) NULL,
        CONSTRAINT [PK_History] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_History_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [InvoiceAttachments] (
        [Id] int NOT NULL IDENTITY,
        [Identifier] nvarchar(max) NULL,
        [FileName] nvarchar(max) NULL,
        [ContentType] nvarchar(max) NULL,
        [Size] bigint NOT NULL,
        [InvoiceId] int NOT NULL,
        CONSTRAINT [PK_InvoiceAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceAttachments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [InvoiceLinks] (
        [Id] int NOT NULL IDENTITY,
        [LinkId] nvarchar(max) NULL,
        [InvoiceId] int NULL,
        [Expired] bit NOT NULL,
        CONSTRAINT [PK_InvoiceLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InvoiceLinks_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [LineItems] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(max) NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [TaxExempt] bit NOT NULL,
        [InvoiceId] int NULL,
        CONSTRAINT [PK_LineItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LineItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE TABLE [PaymentEvents] (
        [Id] int NOT NULL IDENTITY,
        [Processor] nvarchar(max) NULL,
        [ProcessorId] nvarchar(max) NULL,
        [Decision] nvarchar(max) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [BillingFirstName] nvarchar(60) NULL,
        [BillingLastName] nvarchar(60) NULL,
        [BillingEmail] nvarchar(1500) NULL,
        [BillingCompany] nvarchar(60) NULL,
        [BillingPhone] nvarchar(100) NULL,
        [BillingStreet1] nvarchar(400) NULL,
        [BillingStreet2] nvarchar(400) NULL,
        [BillingCity] nvarchar(50) NULL,
        [BillingState] nvarchar(64) NULL,
        [BillingCountry] nvarchar(2) NULL,
        [BillingPostalCode] nvarchar(10) NULL,
        [CardType] nvarchar(3) NULL,
        [CardNumber] nvarchar(20) NULL,
        [CardExpiry] datetime2 NULL,
        [ReturnedResults] nvarchar(max) NULL,
        [OccuredAt] datetime2 NOT NULL,
        [InvoiceId] int NULL,
        CONSTRAINT [PK_PaymentEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentEvents_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_Coupons_TeamId] ON [Coupons] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_FinancialAccounts_TeamId] ON [FinancialAccounts] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_History_InvoiceId] ON [History] ([InvoiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_InvoiceAttachments_InvoiceId] ON [InvoiceAttachments] ([InvoiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_InvoiceLinks_InvoiceId] ON [InvoiceLinks] ([InvoiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_Invoices_AccountId] ON [Invoices] ([AccountId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_Invoices_CouponId] ON [Invoices] ([CouponId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_Invoices_TeamId] ON [Invoices] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_LineItems_InvoiceId] ON [LineItems] ([InvoiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_Logs_JobId] ON [Logs] ([JobId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_PaymentEvents_InvoiceId] ON [PaymentEvents] ([InvoiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_TeamPermissions_RoleId] ON [TeamPermissions] ([RoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_TeamPermissions_TeamId] ON [TeamPermissions] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_TeamPermissions_UserId] ON [TeamPermissions] ([UserId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    CREATE INDEX [IX_WebHooks_TeamId] ON [WebHooks] ([TeamId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250915194445_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250915194445_InitialCreate', N'6.0.3');
END;
GO

COMMIT;
GO

