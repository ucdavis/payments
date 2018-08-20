CREATE TABLE [dbo].[TeamRoles] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_TeamRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);

