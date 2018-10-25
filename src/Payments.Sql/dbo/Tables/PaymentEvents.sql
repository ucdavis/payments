CREATE TABLE [dbo].[PaymentEvents] (
	[Id]				INT				IDENTITY (1, 1) NOT NULL,
    [Processor]     NVARCHAR (50)	NOT NULL,
    [ProcessorId]       NVARCHAR (50)	NULL,
    [Amount]			DECIMAL			NOT NULL,
    [Decision]          NVARCHAR (50)	NOT NULL,
    [OccuredAt]         DATETIME2 (7)	NOT NULL,
    [ReturnedResults]   NVARCHAR (MAX)	NULL,
    [InvoiceId]			INT				NULL, 
    CONSTRAINT [PK_PaymentEvents] PRIMARY KEY CLUSTERED ([Id] ASC), 
    CONSTRAINT [FK_PaymentEvents_Invoices] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices]([Id])
);

