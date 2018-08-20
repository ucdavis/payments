CREATE TABLE [dbo].[PaymentEvents] (
    [Transaction_Id]       NVARCHAR (450) NOT NULL,
    [Auth_Amount]          NVARCHAR (MAX) NULL,
    [Decision]             NVARCHAR (MAX) NULL,
    [OccuredAt]            DATETIME2 (7)  NOT NULL,
    [Reason_Code]          INT            NOT NULL,
    [Req_Reference_Number] INT            NOT NULL,
    [ReturnedResults]      NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_PaymentEvents] PRIMARY KEY CLUSTERED ([Transaction_Id] ASC)
);

