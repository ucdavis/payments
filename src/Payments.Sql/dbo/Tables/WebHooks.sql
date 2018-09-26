CREATE TABLE [dbo].[WebHooks]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[TeamId] INT NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT 1, 
    [Url] NVARCHAR(255) NOT NULL, 
    [ContentType] NVARCHAR(50) NOT NULL, 
    [TriggerOnPaid] BIT NOT NULL DEFAULT 0, 
	CONSTRAINT [PK_WebHooks] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_WebHooks_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id])

)
GO
CREATE NONCLUSTERED INDEX [IX_WebHooks_TeamId]
    ON [dbo].[WebHooks]([TeamId] ASC);