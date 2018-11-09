CREATE TABLE [dbo].[LineItems] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Amount]      DECIMAL (18, 2) NOT NULL,
    [Description] NVARCHAR (MAX)  NULL,
    [InvoiceId]   INT             NULL,
    [Quantity]    DECIMAL (18, 2) NOT NULL,
    [Total]       DECIMAL (18, 2) NOT NULL,
    CONSTRAINT [PK_LineItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LineItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_LineItems_InvoiceId]
    ON [dbo].[LineItems]([InvoiceId] ASC);

