CREATE TABLE [dbo].[InvoiceAttachments]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[Identifier] NVARCHAR(255) NOT NULL, 
    [FileName] NVARCHAR(255) NOT NULL, 
    [ContentType] NVARCHAR(50) NOT NULL, 
    [Size] BIGINT NOT NULL, 
    [InvoiceId] INT NOT NULL, 
    CONSTRAINT [PK_InvoiceAttachments] PRIMARY KEY CLUSTERED ([Id] ASC), 
	CONSTRAINT [PK_InvoiceAttachmentss] PRIMARY KEY CLUSTERED ([Id] ASC),
)
