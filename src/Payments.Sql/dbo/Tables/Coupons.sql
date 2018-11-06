CREATE TABLE [dbo].[Coupons]
(
	[Id]                INT             IDENTITY (1, 1) NOT NULL,
    [TeamId]			INT				NOT NULL, 
	[Name]				NVARCHAR(50)	NOT NULL,
    [Code]				NVARCHAR(50)	NULL,
    [DiscountAmount]	DECIMAL(18, 2)	NULL,
    [DiscountPercent]	DECIMAL(18, 5)	NULL,
    [ExpiresAt]			DATETIME2		NULL,
    CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Coupons_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([Id]),
)
