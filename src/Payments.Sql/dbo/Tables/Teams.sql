CREATE TABLE [dbo].[Teams] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [IsActive]           BIT            NOT NULL,
    [Name]               NVARCHAR (128) NOT NULL,
    [Slug]               NVARCHAR (40)  NOT NULL,
    [ContactName]        NVARCHAR (128) NULL,
    [ContactEmail]       NVARCHAR (128) NULL,
    [ContactPhoneNumber] NVARCHAR (40)  NULL,
    [ApiKey]             NVARCHAR (50)  CONSTRAINT [DF_Teams_ApiKey] DEFAULT (replace(newid(),'-','')) NOT NULL,
    CONSTRAINT [PK_Teams] PRIMARY KEY CLUSTERED ([Id] ASC)
);



