CREATE TABLE [dbo].[History] (
    [Id]		          INT            IDENTITY (1, 1) NOT NULL,
    [InvoiceId]	          INT		     NOT NULL, 
    [ActionDateTime]      DATETIME2 (7)  NOT NULL,
    [Type]				  NVARCHAR (50)  NOT NULL,
    [Actor]			  NVARCHAR (450) NULL,
    [Data]				  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_History] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_History_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[Invoices] ([Id])
);


GO
