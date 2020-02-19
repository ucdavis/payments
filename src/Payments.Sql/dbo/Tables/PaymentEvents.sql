CREATE TABLE [dbo].[PaymentEvents] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [Processor]         NVARCHAR (50)   NOT NULL,
    [ProcessorId]       NVARCHAR (50)   NULL,
    [Amount]            DECIMAL (18, 2) NOT NULL,
    [Decision]          NVARCHAR (50)   NOT NULL,
    [OccuredAt]         DATETIME2 (7)   NOT NULL,
    [ReturnedResults]   NVARCHAR (MAX)  NULL,
    [InvoiceId]         INT             NULL,
    [BillingFirstName]  NVARCHAR (255)   NULL,
    [BillingLastName]   NVARCHAR (255)   NULL,
    [BillingEmail]      NVARCHAR (500)  NULL,
    [BillingCompany]    NVARCHAR (500)   NULL,
    [BillingPhone]      NVARCHAR (255)   NULL,
    [BillingStreet1]    NVARCHAR (500)   NULL,
    [BillingStreet2]    NVARCHAR (500)   NULL,
    [BillingCity]       NVARCHAR (500)   NULL,
    [BillingState]      NVARCHAR (255)    NULL,
    [BillingCountry]    NVARCHAR (255)    NULL,
    [BillingPostalCode] NVARCHAR (255)   NULL,
    [CardType]          NVARCHAR (3)    NULL,
    [CardNumber]        NVARCHAR (20)   NULL,
    [CardExpiry]        DATETIME2 (7)   NULL,
    CONSTRAINT [PK_PaymentEvents] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PaymentEvents_Invoices] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);



