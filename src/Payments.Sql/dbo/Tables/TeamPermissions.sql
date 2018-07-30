CREATE TABLE [dbo].[TeamPermissions] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [RoleId] INT            NOT NULL,
    [TeamId] INT            NOT NULL,
    [UserId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_TeamPermissions] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TeamPermissions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TeamPermissions_TeamRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[TeamRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TeamPermissions_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_TeamPermissions_UserId]
    ON [dbo].[TeamPermissions]([UserId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TeamPermissions_TeamId]
    ON [dbo].[TeamPermissions]([TeamId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TeamPermissions_RoleId]
    ON [dbo].[TeamPermissions]([RoleId] ASC);

