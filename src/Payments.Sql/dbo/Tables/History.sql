CREATE TABLE [dbo].[History] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [ActorId] NVARCHAR (450) NULL,
    [Data]    NVARCHAR (MAX) NULL,
    [Date]    DATETIME2 (7)  NOT NULL,
    [Type]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_History] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_History_AspNetUsers_ActorId] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_History_ActorId]
    ON [dbo].[History]([ActorId] ASC);

