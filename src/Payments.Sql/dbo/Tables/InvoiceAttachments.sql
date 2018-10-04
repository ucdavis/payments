CREATE TABLE [dbo].[InvoiceAttachments]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
    [Identifier] NVARCHAR(255) NOT NULL, 
    [FileName] NVARCHAR(255) NOT NULL, 
    [ContentType] NVARCHAR(50) NOT NULL, 
    [Size] INT NOT NULL, 
    [InvoiceId] INT NULL, 
    [TeamId] INT NULL,
	CONSTRAINT [PK_InvoiceAttachmentss] PRIMARY KEY CLUSTERED ([Id] ASC),
)
