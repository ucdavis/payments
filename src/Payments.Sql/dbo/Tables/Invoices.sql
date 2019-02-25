CREATE TABLE [dbo].[Invoices] (
    [Id]						INT             IDENTITY  (1, 1) NOT NULL,
    [LinkId]					NVARCHAR (MAX)  NULL,
    [TeamId]					INT             NOT NULL,
    [AccountId]					INT             NULL,
	[CouponId]					INT				NULL,
    [CreatedAt]					DATETIME2 (7)   NOT NULL,
    [CustomerAddress]			NVARCHAR (MAX)  NULL,
    [CustomerEmail]				NVARCHAR (MAX)  NULL,
    [CustomerName]				NVARCHAR (MAX)  NULL,
    [DueDate]					DATETIME2 (7)   NULL,
    [DraftCount]				INT				NOT NULL  DEFAULT 0, 
    [Memo]						NVARCHAR (MAX)  NULL,
    [Sent]						BIT             NOT NULL,
    [SentAt]					DATETIME2 (7)   NULL,
    [Status]					NVARCHAR (MAX)  NULL,
    [CalculatedSubtotal]        DECIMAL (18, 2) NOT NULL DEFAULT 0,
    [ManualDiscount]            DECIMAL (18, 2) NOT NULL DEFAULT 0,
    [CalculatedDiscount]        DECIMAL (18, 2) NOT NULL DEFAULT 0,
    [CalculatedTaxableAmount]   DECIMAL (18, 2) NOT NULL DEFAULT 0, 
    [CalculatedTaxAmount]       DECIMAL (18, 2) NOT NULL DEFAULT 0,
    [TaxPercent]				DECIMAL (18, 5) NOT NULL  DEFAULT 0,
    [CalculatedTotal]           DECIMAL (18, 2) NOT NULL DEFAULT 0,
    [Deleted]					BIT				NOT NULL  , 
    [DeletedAt]					DATETIME2		NULL, 
    [Paid]						BIT				NOT NULL, 
    [PaidAt]					DATETIME2		NULL, 
    [PaymentType]				NVARCHAR(50)	NULL, 
    [PaymentProcessorId]		NVARCHAR(150)	NULL, 
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_Coupons_CouponId] FOREIGN KEY ([CouponId]) REFERENCES [dbo].[Coupons] ([Id]),
    CONSTRAINT [FK_Invoices_FinancialAccounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[FinancialAccounts] ([Id]),
    CONSTRAINT [FK_Invoices_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_TeamId]
    ON [dbo].[Invoices]([TeamId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_AccountId]
    ON [dbo].[Invoices]([AccountId] ASC);

