CREATE TABLE [dbo].[InvoiceLinks]
(
	[Id]			INT IDENTITY (1, 1) NOT NULL, 
    [LinkId]		NVARCHAR(50)	NOT NULL, 
    [InvoiceId]		INT				NOT NULL, 
    [Expired]		BIT				NOT NULL, 
	CONSTRAINT [PK_InvoiceLinkss] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InvoiceLinks_Invoices] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices]([Id])
)
