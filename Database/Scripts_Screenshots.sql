
GO

/****** Drop Objects if exist ******/
IF TYPE_ID(N'aut.TestScreenshotType') IS NOT NULL
    DROP TYPE [aut].[TestScreenshotType];
GO

IF OBJECT_ID(N'aut.TestScreenshots', N'U') IS NOT NULL
    DROP TABLE [aut].[TestScreenshots];
GO

IF OBJECT_ID(N'aut.usp_BulkInsertTestScreenshots', N'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_BulkInsertTestScreenshots];
GO

IF OBJECT_ID(N'aut.usp_GetScreenshotById', N'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_GetScreenshotById];
GO

IF OBJECT_ID(N'aut.usp_GetScreenshotsByTestResultId', N'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_GetScreenshotsByTestResultId];
GO

IF OBJECT_ID(N'aut.usp_InsertTestScreenshot', N'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_InsertTestScreenshot];
GO


/****** Object:  UserDefinedTableType [aut].[TestScreenshotType] ******/
CREATE TYPE [aut].[TestScreenshotType] AS TABLE(
    [TestResultId] [int] NULL,
    [QueueId] [int] NULL,
    [Caption] [nvarchar](max) NULL,
    [Screenshot] [varbinary](max) NULL,
    [TakenAt] [datetime] NULL
);
GO

/****** Object:  Table [aut].[TestScreenshots] ******/
CREATE TABLE [aut].[TestScreenshots](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [TestResultId] [int] NOT NULL,
    [QueueId] [int] NOT NULL,
    [Caption] [nvarchar](max) NULL,
    [Screenshot] [varbinary](max) NOT NULL,
    [TakenAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, 
       IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, 
       ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) 
       ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
GO

ALTER TABLE [aut].[TestScreenshots] 
    ADD DEFAULT (getutcdate()) FOR [TakenAt];
GO

/****** Object:  StoredProcedure [aut].[usp_BulkInsertTestScreenshots] ******/
CREATE PROCEDURE [aut].[usp_BulkInsertTestScreenshots]
    @Screenshots [aut].[TestScreenshotType] READONLY
AS
BEGIN
    INSERT INTO [aut].[TestScreenshots] (TestResultId, QueueId, Caption, Screenshot, TakenAt)
    SELECT 
        TestResultId,
        QueueId,
        Caption,
        Screenshot,
        ISNULL(TakenAt, GETUTCDATE())
    FROM @Screenshots;
END;
GO

/****** Object:  StoredProcedure [aut].[usp_GetScreenshotById] ******/
CREATE PROCEDURE [aut].[usp_GetScreenshotById]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [ID],
        [TestResultId],
        [QueueId],
        [Caption],
        [Screenshot],
        [TakenAt]
    FROM [aut].[TestScreenshots]
    WHERE [ID] = @ID;
END;
GO

/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByTestResultId] ******/
CREATE PROCEDURE [aut].[usp_GetScreenshotsByTestResultId]
    @TestResultId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [ID],
        [TestResultId],
        [QueueId],
        [Caption],
        [Screenshot],
        [TakenAt]
    FROM [aut].[TestScreenshots]
    WHERE [TestResultId] = @TestResultId
    ORDER BY [TakenAt] DESC;
END;
GO

/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot] ******/
CREATE PROCEDURE [aut].[usp_InsertTestScreenshot]
    @TestResultId INT,
    @QueueId INT,
    @Caption NVARCHAR(MAX),
    @Screenshot VARBINARY(MAX),
    @TakenAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [aut].[TestScreenshots]
    (
        TestResultId,
        QueueId,
        Caption,
        Screenshot,
        TakenAt
    )
    VALUES
    (
        @TestResultId,
        @QueueId,
        @Caption,
        @Screenshot,
        ISNULL(@TakenAt, GETUTCDATE())
    );
END;
GO
