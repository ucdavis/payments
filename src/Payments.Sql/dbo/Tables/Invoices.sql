CREATE TABLE [dbo].[Invoices] (
    [Id]                    INT             IDENTITY (1, 1) NOT NULL,
    [AccountId]             INT             NULL,
    [CreatedAt]             DATETIME2 (7)   NOT NULL,
    [CustomerAddress]       NVARCHAR (MAX)  NULL,
    [CustomerEmail]         NVARCHAR (MAX)  NULL,
    [CustomerName]          NVARCHAR (MAX)  NULL,
    [Discount]              DECIMAL (18, 2) NOT NULL,
    [DueDate]               DATETIME2 (7)   NULL,
    [LinkId]                NVARCHAR (MAX)  NULL,
    [Memo]                  NVARCHAR (MAX)  NULL,
    [PaymentTransaction_Id] NVARCHAR (450)  NULL,
    [Sent]                  BIT             NOT NULL,
    [SentAt]                DATETIME2 (7)   NULL,
    [Status]                NVARCHAR (MAX)  NULL,
    [Subtotal]              DECIMAL (18, 2) NOT NULL,
    [TaxAmount]             DECIMAL (18, 2) NOT NULL,
    [TaxPercent]            DECIMAL (18, 5) NOT NULL,
    [TeamId]                INT             NOT NULL,
    [Total]                 DECIMAL (18, 2) NOT NULL,
    [Deleted]				BIT				NOT NULL DEFAULT 0, 
    [DeletedAt]				DATETIME2		NULL, 
    CONSTRAINT [PK_Invoices] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Invoices_FinancialAccounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[FinancialAccounts] ([Id]),
    CONSTRAINT [FK_Invoices_PaymentEvents_PaymentTransaction_Id] FOREIGN KEY ([PaymentTransaction_Id]) REFERENCES [dbo].[PaymentEvents] ([Transaction_Id]),
    CONSTRAINT [FK_Invoices_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_TeamId]
    ON [dbo].[Invoices]([TeamId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_PaymentTransaction_Id]
    ON [dbo].[Invoices]([PaymentTransaction_Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_AccountId]
    ON [dbo].[Invoices]([AccountId] ASC);

