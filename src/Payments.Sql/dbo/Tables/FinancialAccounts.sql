CREATE TABLE [dbo].[FinancialAccounts] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Account]     NVARCHAR (7)   NULL,
    [Chart]       NVARCHAR (1)   NULL,
    [Description] NVARCHAR (MAX) NULL,
    [IsActive]    BIT            NOT NULL,
    [IsDefault]   BIT            NOT NULL,
    [Name]        NVARCHAR (128) NOT NULL,
    [Project]     NVARCHAR (9)   NULL,
    [SubAccount]  NVARCHAR (5)   NULL,
    [TeamId]      INT            NOT NULL,
    [FinancialSegmentString] NVARCHAR(128) NULL, 
    CONSTRAINT [PK_FinancialAccounts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_FinancialAccounts_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FinancialAccounts_TeamId]
    ON [dbo].[FinancialAccounts]([TeamId] ASC);

