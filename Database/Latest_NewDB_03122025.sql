USE [master]
GO
/****** Object:  Database [MES_AUT]    Script Date: 03-12-2025 00:33:01 ******/
CREATE DATABASE [MES_AUT]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'MES_AUT', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\MES_AUT.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'MES_AUT_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\MES_AUT_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [MES_AUT] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [MES_AUT].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [MES_AUT] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [MES_AUT] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [MES_AUT] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [MES_AUT] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [MES_AUT] SET ARITHABORT OFF 
GO
ALTER DATABASE [MES_AUT] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [MES_AUT] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [MES_AUT] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [MES_AUT] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [MES_AUT] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [MES_AUT] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [MES_AUT] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [MES_AUT] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [MES_AUT] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [MES_AUT] SET  DISABLE_BROKER 
GO
ALTER DATABASE [MES_AUT] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [MES_AUT] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [MES_AUT] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [MES_AUT] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [MES_AUT] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [MES_AUT] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [MES_AUT] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [MES_AUT] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [MES_AUT] SET  MULTI_USER 
GO
ALTER DATABASE [MES_AUT] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [MES_AUT] SET DB_CHAINING OFF 
GO
ALTER DATABASE [MES_AUT] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [MES_AUT] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [MES_AUT] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [MES_AUT] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [MES_AUT] SET QUERY_STORE = ON
GO
ALTER DATABASE [MES_AUT] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [MES_AUT]
GO
/****** Object:  User [automation_user]    Script Date: 03-12-2025 00:33:02 ******/
CREATE USER [automation_user] FOR LOGIN [automation_user] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [automation_user]
GO
ALTER ROLE [db_datareader] ADD MEMBER [automation_user]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [automation_user]
GO
/****** Object:  Schema [aut]    Script Date: 03-12-2025 00:33:02 ******/
CREATE SCHEMA [aut]
GO
/****** Object:  UserDefinedTableType [aut].[AssignmentTestCaseIdList]    Script Date: 03-12-2025 00:33:02 ******/
CREATE TYPE [aut].[AssignmentTestCaseIdList] AS TABLE(
	[AssignmentTestCaseId] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [aut].[TestCaseType]    Script Date: 03-12-2025 00:33:02 ******/
CREATE TYPE [aut].[TestCaseType] AS TABLE(
	[TestCaseId] [nvarchar](200) NULL,
	[TestCaseDescription] [nvarchar](500) NULL,
	[ClassName] [nvarchar](500) NULL,
	[LibraryName] [nvarchar](500) NULL,
	[MethodName] [nvarchar](500) NULL,
	[Priority] [nvarchar](50) NULL,
	[TestCaseStatus] [nvarchar](50) NULL
)
GO
/****** Object:  UserDefinedTableType [aut].[TestScreenshotType]    Script Date: 03-12-2025 00:33:02 ******/
CREATE TYPE [aut].[TestScreenshotType] AS TABLE(
	[AssignmentTestCaseId] [int] NULL,
	[Caption] [nvarchar](max) NULL,
	[Screenshot] [varbinary](max) NULL,
	[TakenAt] [datetime] NULL
)
GO
/****** Object:  Table [aut].[AssignedTestCases]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AssignedTestCases](
	[AssignmentTestCaseId] [int] IDENTITY(1,1) NOT NULL,
	[AssignmentId] [int] NOT NULL,
	[TestCaseId] [nvarchar](100) NULL,
	[TestCaseDescription] [nvarchar](max) NULL,
	[TestCaseStatus] [nvarchar](100) NOT NULL,
	[ClassName] [nvarchar](255) NULL,
	[LibraryName] [nvarchar](255) NULL,
	[MethodName] [nvarchar](255) NULL,
	[Priority] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Duration] [float] NULL,
	[ErrorMessage] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[AssignmentTestCaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[AutomationData]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AutomationData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SectionID] [int] NULL,
	[TestContent] [varchar](max) NULL,
	[UserID] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[AutomationDataSections]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AutomationDataSections](
	[SectionID] [int] IDENTITY(1,1) NOT NULL,
	[SectionName] [nvarchar](250) NULL,
	[FlowName] [nvarchar](250) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[PriorityStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[PriorityStatus](
	[PriorityID] [int] NOT NULL,
	[PriorityName] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PriorityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseAssignment]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseAssignment](
	[AssignmentId] [int] IDENTITY(1,1) NOT NULL,
	[AssignmentName] [nvarchar](255) NOT NULL,
	[AssignmentStatus] [nvarchar](100) NOT NULL,
	[AssignedUser] [int] NOT NULL,
	[ReleaseName] [nvarchar](255) NOT NULL,
	[Environment] [nvarchar](100) NOT NULL,
	[AssignedDate] [datetime] NOT NULL,
	[AssignedBy] [int] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseExecutionQueue]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseExecutionQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[QueueId] [uniqueidentifier] NOT NULL,
	[AssignmentId] [int] NOT NULL,
	[AssignmentTestCaseId] [int] NOT NULL,
	[QueueStatus] [nvarchar](100) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ExecutionDateTime] [datetime] NULL,
	[Browser] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_TestCaseExecutionQueue_QueueId] UNIQUE NONCLUSTERED 
(
	[QueueId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestScreenshots]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestScreenshots](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Caption] [nvarchar](max) NULL,
	[Screenshot] [varbinary](max) NOT NULL,
	[TakenAt] [datetime] NULL,
	[AssignmentTestCaseId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[TimeZone]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TimeZone](
	[TimeZoneID] [int] IDENTITY(1,1) NOT NULL,
	[TimeZoneName] [nvarchar](100) NOT NULL,
	[UTCOffsetMinutes] [int] NOT NULL,
	[Description] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[TimeZoneID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[User]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Photo] [varbinary](max) NULL,
	[RoleID] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[TwoFactorEnabled] [bit] NULL,
	[TeamsProjects] [nvarchar](255) NULL,
	[Priority] [int] NULL,
	[Status] [int] NULL,
	[LastLogin] [datetime] NULL,
	[TimeZone] [int] NULL,
	[PasswordHash] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[UserRole]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[UserRole](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[UserStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[UserStatus](
	[StatusID] [int] NOT NULL,
	[StatusName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [aut].[TestCaseExecutionQueue] ADD  DEFAULT (newid()) FOR [QueueId]
GO
ALTER TABLE [aut].[TestScreenshots] ADD  DEFAULT (getutcdate()) FOR [TakenAt]
GO
ALTER TABLE [aut].[User] ADD  CONSTRAINT [DF_User_Active]  DEFAULT ((1)) FOR [Active]
GO
ALTER TABLE [aut].[User] ADD  CONSTRAINT [DF_User_TwoFactorEnabled]  DEFAULT ((0)) FOR [TwoFactorEnabled]
GO
ALTER TABLE [aut].[AssignedTestCases]  WITH CHECK ADD  CONSTRAINT [FK_AssignedTestCases_Assignment] FOREIGN KEY([AssignmentId])
REFERENCES [aut].[TestCaseAssignment] ([AssignmentId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[AssignedTestCases] CHECK CONSTRAINT [FK_AssignedTestCases_Assignment]
GO
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_AssignedTestCase] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
GO
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_AssignedTestCase]
GO
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_Assignment] FOREIGN KEY([AssignmentId])
REFERENCES [aut].[TestCaseAssignment] ([AssignmentId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_Assignment]
GO
ALTER TABLE [aut].[TestScreenshots]  WITH CHECK ADD  CONSTRAINT [FK_TestScreenshots_AssignedTestCases] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[TestScreenshots] CHECK CONSTRAINT [FK_TestScreenshots_AssignedTestCases]
GO
ALTER TABLE [aut].[User]  WITH CHECK ADD FOREIGN KEY([RoleID])
REFERENCES [aut].[UserRole] ([RoleID])
GO
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Priority] FOREIGN KEY([Priority])
REFERENCES [aut].[PriorityStatus] ([PriorityID])
GO
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_Priority]
GO
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Status] FOREIGN KEY([Status])
REFERENCES [aut].[UserStatus] ([StatusID])
GO
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_Status]
GO
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_TimeZone] FOREIGN KEY([TimeZone])
REFERENCES [aut].[TimeZone] ([TimeZoneID])
GO
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_TimeZone]
GO
/****** Object:  StoredProcedure [aut].[usp_BulkInsertTestScreenshots]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--------------------------------------------------------------------------------
-- 6. Create the updated stored procedures
--------------------------------------------------------------------------------

-- Bulk insert screenshots
CREATE PROCEDURE [aut].[usp_BulkInsertTestScreenshots]
    @Screenshots aut.TestScreenshotType READONLY
AS
BEGIN
    INSERT INTO aut.TestScreenshots 
        (AssignmentTestCaseId, Caption, Screenshot, TakenAt)
    SELECT 
        AssignmentTestCaseId, Caption, Screenshot,
        ISNULL(TakenAt, GETUTCDATE())
    FROM @Screenshots;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_BulkRunTestCasesNow]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_BulkRunTestCasesNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
	@Browser VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        ---------------------------------------------------------
        -- 1️⃣ Insert Multiple Queue Rows (Bulk Run)
        ---------------------------------------------------------
        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
			Browser
        )
        SELECT 
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
			@Browser
        FROM @AssignmentTestCaseIds;

        ---------------------------------------------------------
        -- 2️⃣ Update ALL Assigned Test Cases as Queued
        ---------------------------------------------------------
        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

        ---------------------------------------------------------
        -- 3️⃣ Return simple success flag
        ---------------------------------------------------------
        SELECT 1 AS Success;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;

        THROW; -- Return original SQL error
    END CATCH

END;
GO
/****** Object:  StoredProcedure [aut].[usp_BulkScheduleTestCases]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_BulkScheduleTestCases]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
    @ScheduleDate DATETIME,
	@Browser VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        ---------------------------------------------------------
        -- 1️⃣ Insert Queue Rows (Bulk Schedule)
        ---------------------------------------------------------
        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
			Browser
        )
        SELECT 
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
			@Browser
        FROM @AssignmentTestCaseIds;

        ---------------------------------------------------------
        -- 2️⃣ Update Assigned Test Cases as Scheduled
        ---------------------------------------------------------
        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

        ---------------------------------------------------------
        -- 3️⃣ Return Success
        ---------------------------------------------------------
        SELECT 1 AS Success;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH

END;
GO
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_ChangePassword]
    @UserID INT,
    @OldPassword NVARCHAR(255),
    @NewPassword NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if user exists and old password is correct
    IF EXISTS (
        SELECT 1 FROM [aut].[User]
        WHERE UserID = @UserID AND Password = @OldPassword
    )
    BEGIN
        -- Update password
        UPDATE [aut].[User]
        SET Password = @NewPassword
        WHERE UserID = @UserID;

        SELECT 1 AS StatusCode, 'Password changed successfully.' AS Message;
    END
    ELSE
    BEGIN
        SELECT 0 AS StatusCode, 'Error: Incorrect current password or user not found.' AS Message;
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_CreateOrUpdateAssignmentWithTestCases]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_CreateOrUpdateAssignmentWithTestCases]
(
    @AssignmentStatus NVARCHAR(100),
    @AssignedUser INT,
    @ReleaseName NVARCHAR(255),
    @Environment NVARCHAR(100),
    @AssignedDate DATETIME,
    @AssignedBy INT,
    @LastUpdatedDate DATETIME,
    @TestCases aut.TestCaseType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExistingAssignmentId INT;
    DECLARE @TestCaseCount INT;
    DECLARE @TesterName NVARCHAR(255);
    DECLARE @AssignmentName NVARCHAR(255);

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Generate AssignmentName = TesterName-Release-Environment
        SELECT @TesterName = UserName 
        FROM aut.[User] 
        WHERE UserID = @AssignedUser;

        SET @AssignmentName = @TesterName + '-' + @ReleaseName + '-' + @Environment;

        -- Check if assignment exists
        SELECT @ExistingAssignmentId = AssignmentId
        FROM aut.TestCaseAssignment
        WHERE AssignmentName = @AssignmentName
          AND AssignedUser = @AssignedUser;

        -- Insert if assignment not exists
        IF @ExistingAssignmentId IS NULL
        BEGIN
            SELECT @TestCaseCount = COUNT(*) FROM @TestCases;
            IF @TestCaseCount = 0
            BEGIN
                ROLLBACK TRANSACTION;
                RETURN;
            END

            INSERT INTO aut.TestCaseAssignment
            (
                AssignmentName,
                AssignmentStatus,
                AssignedUser,
                ReleaseName,
                Environment,
                AssignedDate,
                AssignedBy,
                LastUpdatedDate
            )
            VALUES
            (
                @AssignmentName,
                @AssignmentStatus,
                @AssignedUser,
                @ReleaseName,
                @Environment,
                @AssignedDate,
                @AssignedBy,
                @LastUpdatedDate
            );

            SET @ExistingAssignmentId = SCOPE_IDENTITY();
        END

        -- Delete removed test cases
        DELETE ATC
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND NOT EXISTS (
                SELECT 1 
                FROM @TestCases TC
                WHERE ATC.TestCaseId = TC.TestCaseId
           );

        -- Insert / update test cases
        MERGE aut.AssignedTestCases AS Target
        USING @TestCases AS Source
        ON Target.AssignmentId = @ExistingAssignmentId 
           AND Target.TestCaseId = Source.TestCaseId
        WHEN MATCHED THEN
            UPDATE SET
                TestCaseDescription = Source.TestCaseDescription,
				TestCaseStatus = Source.TestCaseStatus,
                ClassName = Source.ClassName,
                LibraryName = Source.LibraryName,
                MethodName = Source.MethodName,
                Priority = Source.Priority
        WHEN NOT MATCHED BY TARGET THEN
            INSERT
            (
                AssignmentId,
                TestCaseId,
                TestCaseDescription,
				TestCaseStatus,
                ClassName,
                LibraryName,
                MethodName,
                Priority
            )
            VALUES
            (
                @ExistingAssignmentId,
                Source.TestCaseId,
                Source.TestCaseDescription,
				source.TestCaseStatus,
                Source.ClassName,
                Source.LibraryName,
                Source.MethodName,
                Source.Priority
            );

        -- If no test cases, delete assignment
        SELECT @TestCaseCount = COUNT(*) 
        FROM aut.AssignedTestCases 
        WHERE AssignmentId = @ExistingAssignmentId;

        IF @TestCaseCount = 0
        BEGIN
            DELETE FROM aut.TestCaseAssignment 
            WHERE AssignmentId = @ExistingAssignmentId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_CreateUser]
    @UserName       NVARCHAR(100),
    @Password       NVARCHAR(255),
    @FirstName      NVARCHAR(100) = NULL,
    @LastName       NVARCHAR(100) = NULL,
    @Email          NVARCHAR(255),
    @Photo          VARBINARY(MAX) = NULL,
    @RoleID         INT, 
    @Active         BIT = 1,           
    @TimeZone       INT = NULL,
    @TwoFactor      BIT = 0,          
    @Teams          NVARCHAR(255) = NULL,
    @PhoneNumber    NVARCHAR(20) = NULL,
	@Priority       INT = NULL,
	@Status         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [aut].[User] 
    (
        UserName,
        Password,
        FirstName,
        LastName,
        Email,
        Photo,
        RoleID,
        Active,
		CreatedAt,
        TimeZone,
        TwoFactorEnabled,
        TeamsProjects,
        PhoneNumber,
		Priority,
        Status
    )
    VALUES 
    (
        @UserName,
        @Password,
        @FirstName,
        @LastName,
        @Email,
        @Photo,
        @RoleID,
        @Active,
		GETDATE(),
        @TimeZone,
        @TwoFactor,
        @Teams,
        @PhoneNumber,
		@Priority,
        @Status
    );

    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationData]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_DeleteAutomationData] @SectionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE [aut].[AutomationData]
	WHERE SectionID = @SectionID
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSection]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_DeleteAutomationDataSection] @SectionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE [aut].[AutomationDataSections]
	WHERE SectionID = @SectionID
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestScreenshotById]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_DeleteTestScreenshotById]
    @ID INT
AS
BEGIN
    DELETE FROM [aut].[TestScreenshots]
    WHERE ID = @ID;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_DeleteUser]
    @UserID INT
AS
BEGIN
    DELETE FROM [aut].[User]
    WHERE UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_get_AutomationDataSection]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [aut].[usp_get_AutomationDataSection]   
  @FlowName nvarchar(200)=NULL
AS  
BEGIN  
SET NOCOUNT ON;  
IF @FlowName IS NULL OR DATALENGTH(@FlowName)=0
BEGIN
Select * from [aut].[AutomationDataSections]
END
ELSE
BEGIN
Select * from [aut].[AutomationDataSections] where FlowName=@FlowName
END
END 

 
/****** Object:  StoredProcedure [aut].[usp_Insert_AutomationData]    Script Date: 11/20/2024 9:46:14 AM ******/
SET ANSI_NULLS ON
GO
/****** Object:  StoredProcedure [aut].[usp_get_AutomationFlowNames]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [aut].[usp_get_AutomationFlowNames]   
  
AS  
BEGIN  
SET NOCOUNT ON;  
  
Select Distinct FlowName from [aut].[AutomationDataSections]
   
END  
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllAssignedTestCasesInLibrary]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetAllAssignedTestCasesInLibrary]
(
    @LibraryName NVARCHAR(255)    
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TC.AssignmentTestCaseId,
        TC.AssignmentId,
        TC.TestCaseId,
        TC.TestCaseDescription,
        TC.TestCaseStatus,
        TC.ClassName,
        TC.LibraryName,
        TC.MethodName,
        TC.Priority,
        TC.StartTime,
        TC.EndTime,
        TC.Duration,
        TC.ErrorMessage,
        U.UserID AS AssignedUserId,
        U.UserName AS AssignedUserName,
        A.AssignmentName,
        A.Environment   -- New column
    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE 
        TC.LibraryName = @LibraryName        
    ORDER BY 
        TC.Priority,
        TC.TestCaseId;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetAllUsers]
AS
BEGIN
    SELECT 
        usr.UserID,
        usr.UserName,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
		usr.Active,
        role.RoleName,
		role.RoleID,
		usr.[Priority],
		priority.PriorityName,
		usr.TimeZone,
		tz.TimeZoneName,
		usr.LastLogin,
		usr.[Status],
		us.[StatusName],
		usr.[PhoneNumber],
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM [aut].[User] AS usr
    LEFT JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
	LEFT JOIN [aut].[PriorityStatus] AS priority ON usr.Priority = priority.PriorityID
	LEFT JOIN [aut].TimeZone AS tz on usr.TimeZone = tz.TimeZoneID
	LEFT JOIN [aut].[UserStatus] AS us on us.StatusID = usr.[Status];
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]
(
    @LibraryName NVARCHAR(255),
    @Environment NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TC.AssignmentTestCaseId,
        TC.AssignmentId,
        TC.TestCaseId,
        TC.TestCaseDescription,
        TC.TestCaseStatus,
        TC.ClassName,
        TC.LibraryName,
        TC.MethodName,
        TC.Priority,
        TC.StartTime,
        TC.EndTime,
        TC.Duration,
        TC.ErrorMessage,
        U.UserID AS AssignedUserId,
        U.UserName AS AssignedUserName,
        A.AssignmentName,
        A.Environment   -- New column
    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE 
        TC.LibraryName = @LibraryName
        AND A.Environment = @Environment  -- Filter by Environment
    ORDER BY 
        TC.Priority,
        TC.TestCaseId;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationData]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAutomationData]
    @SectionID INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [ID],
        [SectionID],
        [TestContent],
        [UserID]
    FROM [aut].[AutomationData] WITH (NOLOCK)
    WHERE SectionID = @SectionID
      AND UserID = @UserId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataByFlowName]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetAutomationDataByFlowName] @FlowName NVARCHAR(150) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT AD.SectionID
		,AD.TestContent
		,AD.ID
		,SC.SectionName
	FROM [aut].[AutomationData] AD WITH (NOLOCK)
	INNER JOIN [aut].[AutomationDataSections] SC ON AD.SectionID = SC.SectionID
	WHERE SC.FlowName = @FlowName
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataSection]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAutomationDataSection]
(
    @FlowName NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Optional: Validate required input
    IF @FlowName IS NULL OR LTRIM(RTRIM(@FlowName)) = ''
    BEGIN
        RAISERROR('FlowName is required.', 16, 1);
        RETURN;
    END

    -- Fetch sections by FlowName
    SELECT
	   [SectionID]
      ,[SectionName]
      ,[FlowName]
    FROM [aut].[AutomationDataSections] WITH (NOLOCK)
    WHERE FlowName = @FlowName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationFlowNames]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetAutomationFlowNames]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT FlowName
	FROM [aut].[AutomationDataSections] WITH (NOLOCK)
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetFilteredUsers]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************************************************
Project Name: OHPNM - Automation Portal
SP Name: aut.[usp_GetFilteredUsers]
Purpose: This sp is used to get users based on search filters
Initial Creator: Shikha Aggarwal
Initial Creation Date: 15-Oct-2025
***********************************************************************************************************/

CREATE PROCEDURE [aut].[usp_GetFilteredUsers]
    @Search NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Role INT = NULL,
    @Priority INT = NULL
AS
BEGIN
    SELECT 
	    usr.UserID,
        usr.UserName,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
		usr.Active,
        ur.RoleName,
		usr.RoleID,
		usr.[Priority],
		up.PriorityName,
		usr.TimeZone,
		tz.TimeZoneName,
		usr.LastLogin,
		usr.[Status],
		us.[StatusName],
		usr.[PhoneNumber],
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM aut.[User] usr
    LEFT JOIN [aut].[UserRole] AS ur ON usr.RoleID = ur.RoleID
    LEFT JOIN [aut].[PriorityStatus] AS up ON usr.Priority = up.PriorityID
    LEFT JOIN [aut].[TimeZone] AS tz ON usr.TimeZone = tz.TimeZoneID
    LEFT JOIN [aut].[UserStatus] AS us ON us.StatusID = usr.[Status]
    WHERE
	(
        @Search IS NULL OR  @Search = '' OR
        (LTRIM(RTRIM(usr.FirstName)) + ' ' + LTRIM(RTRIM(usr.LastName))) LIKE '%' + @Search + '%' OR 
        usr.Email LIKE '%' + @Search + '%'
    )
    AND ((@Status IS NULL OR @Status = 0 OR usr.Status = @Status)
    AND (@Role IS NULL OR @Role = 0 OR usr.RoleID = @Role)
    AND (@Priority IS NULL OR @Priority = 0 OR usr.Priority = @Priority))
END


GO
/****** Object:  StoredProcedure [aut].[usp_GetPendingExecutionQueues]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_GetPendingExecutionQueues]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Q.QueueId,
        Q.AssignmentTestCaseId,
        ATC.LibraryName,
        ATC.ClassName,
        ATC.MethodName,
        TCA.Environment,
		Q.Browser,
        Q.QueueStatus,
        Q.ExecutionDateTime

    FROM aut.TestCaseExecutionQueue Q
    INNER JOIN aut.AssignedTestCases ATC
        ON Q.AssignmentTestCaseId = ATC.AssignmentTestCaseId
    INNER JOIN aut.TestCaseAssignment TCA
        ON ATC.AssignmentId = TCA.AssignmentId   -- ✅ Join to get Environment

    WHERE 
        Q.QueueStatus = 'Queued'
        OR (Q.QueueStatus = 'Scheduled' AND Q.ExecutionDateTime <= GETDATE())

    ORDER BY 
        Q.CreatedDate ASC;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetPriorityStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetPriorityStatus]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        PriorityID,
        PriorityName
    FROM [aut].[PriorityStatus]
    ORDER BY PriorityName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotById]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get screenshot by ID
CREATE PROCEDURE [aut].[usp_GetScreenshotById]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, AssignmentTestCaseId ,Caption, Screenshot, TakenAt
    FROM aut.TestScreenshots
    WHERE ID = @ID;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByAssignmentTestCaseId]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get all screenshots by QueueId and optionally MethodName
CREATE PROCEDURE [aut].[usp_GetScreenshotsByAssignmentTestCaseId]
    @AssignmentTestCaseId NVARCHAR(MAX)    
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, AssignmentTestCaseId,Caption, Screenshot, TakenAt
    FROM aut.TestScreenshots
    WHERE AssignmentTestCaseId = @AssignmentTestCaseId    
    ORDER BY TakenAt DESC;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseAssignmentsByUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE   PROCEDURE [aut].[usp_GetTestCaseAssignmentsByUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate required parameter
    IF (@UserId IS NULL OR @UserId <= 0)
    BEGIN
        RAISERROR('UserId is required and must be greater than zero.', 16, 1);
        RETURN;
    END

    BEGIN TRY

        SELECT 
            A.AssignmentId,
            A.AssignmentName,
            A.AssignmentStatus,
            A.AssignedUser,
            U.UserName AS AssignedUserName,
            A.ReleaseName,
            A.Environment,
            A.AssignedDate,
            A.AssignedBy,
            UB.UserName AS AssignedByUserName,
            A.LastUpdatedDate
        FROM aut.TestCaseAssignment A
        INNER JOIN aut.[User] U 
            ON A.AssignedUser = U.UserID
        INNER JOIN aut.[User] UB 
            ON A.AssignedBy = UB.UserID
        WHERE A.AssignedUser = @UserId;

    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestCasesByAssignmentNameAndUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetTestCasesByAssignmentNameAndUser]
(
    @AssignmentName NVARCHAR(255),
    @AssignedUser INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TC.AssignmentTestCaseId,
        TC.AssignmentId,
        TC.TestCaseId,
        TC.TestCaseDescription,
        TC.TestCaseStatus,
        TC.ClassName,
        TC.LibraryName,
        TC.MethodName,
        TC.Priority,
        TC.StartTime,
        TC.EndTime,
        TC.Duration,
        TC.ErrorMessage,
        U.UserID AS AssignedUserId,
        U.UserName AS AssignedUserName,
		A.Environment
    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE A.AssignmentName = @AssignmentName
      AND A.AssignedUser = @AssignedUser
    ORDER BY TC.Priority, TC.TestCaseId;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetTimeZones]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetTimeZones]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TimeZoneID,
        TimeZoneName,
        UTCOffsetMinutes,
        Description
    FROM [aut].[TimeZone]
    ORDER BY TimeZoneName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetUserById]
    @UserID INT
AS
BEGIN
    SELECT 
        usr.UserID,
        usr.UserName,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
		usr.Active,
        role.RoleName,
		role.RoleID,
		usr.TimeZone,
		tz.TimeZoneName,
		usr.[PhoneNumber],
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
	LEFT JOIN [aut].TimeZone AS tz on usr.TimeZone = tz.TimeZoneID
	LEFT JOIN [aut].[UserStatus] AS us on us.StatusID = usr.[Status]
    WHERE usr.UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserRoles]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetUserRoles]
AS
BEGIN
    SELECT 
        RoleID,
		RoleName
    FROM [aut].[UserRole]    
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserStatuses]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetUserStatuses]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        StatusID,
        StatusName
    FROM [aut].[UserStatus]
    ORDER BY StatusName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationData]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_InsertAutomationData]
(
    @SectionID INT = NULL,
    @TestContent NVARCHAR(MAX) = NULL,
    @UserID INT = NULL
)
AS
BEGIN TRANSACTION

BEGIN TRY
    -- Insert
    INSERT INTO [aut].[AutomationData] (
        [SectionID],
        [TestContent],
        [UserID]
    )
    VALUES (
        @SectionID,
        @TestContent,
        @UserID
    );

    -- Return the new ID
    SELECT SCOPE_IDENTITY();

    COMMIT TRANSACTION
END TRY

BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);

    ROLLBACK TRANSACTION
END CATCH;
GO
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationDataSection]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [aut].[usp_InsertAutomationDataSection] (
	@SectionName AS NVARCHAR(500) = NULL
	,@FlowName AS NVARCHAR(500) = NULL
	)
AS
BEGIN TRANSACTION

BEGIN TRY
	-- insert
	INSERT INTO [aut].[AutomationDataSections] (
		[SectionName]
		,[FlowName]
		)
	VALUES (
		@SectionName
		,@FlowName
		)

	-- Return the new ID
	SELECT SCOPE_IDENTITY();

	COMMIT TRANSACTION
END TRY

BEGIN CATCH
	DECLARE @ErrorMessage NVARCHAR(4000);
	DECLARE @ErrorSeverity INT;
	DECLARE @ErrorState INT;

	SELECT @ErrorMessage = ERROR_MESSAGE()
		,@ErrorSeverity = ERROR_SEVERITY()
		,@ErrorState = ERROR_STATE();

	RAISERROR (
			@ErrorMessage
			,@ErrorSeverity
			,@ErrorState
			);

	ROLLBACK TRANSACTION
END CATCH;
GO
/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Insert single screenshot
CREATE PROCEDURE [aut].[usp_InsertTestScreenshot]
    @AssignmentTestCaseId INT,    
    @Caption NVARCHAR(MAX),
    @Screenshot VARBINARY(MAX),
    @TakenAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.TestScreenshots
        (AssignmentTestCaseId,Caption, Screenshot, TakenAt)
    VALUES
        (@AssignmentTestCaseId,@Caption, @Screenshot, ISNULL(@TakenAt, GETUTCDATE()));
END;
GO
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_RegisterUser]
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleID INT;
    DECLARE @StatusID INT;

    -- Default Role = Viewer
    SELECT @RoleID = RoleID 
    FROM [aut].[UserRole]
    WHERE RoleName = 'Viewer';

    IF (@RoleID IS NULL)
    BEGIN
        THROW 50001, 'Default role ''Viewer'' not found in UserRole table.', 1;
        RETURN;
    END;

    -- Default Status = Suspended
    SELECT @StatusID = StatusID
    FROM [aut].[UserStatus]
    WHERE StatusName = 'Suspended';

    IF (@StatusID IS NULL)
    BEGIN
        THROW 50002, 'Default status ''Suspended'' not found in UserStatus table.', 1;
        RETURN;
    END;

    -- Insert User
    INSERT INTO [aut].[User]
    (
        UserName,
        Email,
        Password,
        FirstName,
        LastName,
        RoleID,
        Status,
        Active,
        CreatedAt
    )
    VALUES
    (
        @Username,
        @Email,
        @Password,
        @Username,     -- FirstName = Username 
        '',            -- LastName empty
        @RoleID,
        @StatusID,
        1,             -- Active
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ScheduleSingleTestCase]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_ScheduleSingleTestCase]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @ScheduleDate DATETIME,
	@Browser VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        ---------------------------------------------------------
        -- 1️⃣ Insert into Queue Table
        ---------------------------------------------------------
        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
			Browser
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
			@Browser
        );

        ---------------------------------------------------------
        -- 2️⃣ Update Assigned Test Case Status
        ---------------------------------------------------------
        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        ---------------------------------------------------------
        -- 3️⃣ Return Queue Entry Details
        ---------------------------------------------------------
        SELECT  
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;

        THROW;  -- Returns full SQL error
    END CATCH

END;
GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_SetUserActiveStatus]
    @UserID INT,
    @Active BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if user exists (optional, for validation)
    IF EXISTS (SELECT 1 FROM [aut].[User] WHERE UserId = @UserID)
    BEGIN
        UPDATE [aut].[User]
        SET Active = @Active
        WHERE UserId = @UserID;
    END
    ELSE
    BEGIN
        -- Optional: Return an error message
        RAISERROR('User with UserId %d not found.', 16, 1, @UserID);
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_SingleRunTestCaseNow]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_SingleRunTestCaseNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
	@Browser VARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        ---------------------------------------------------------
        -- 1️⃣ Insert Queue Entry
        ---------------------------------------------------------
        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
			Browser
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
			@Browser
        );

        ---------------------------------------------------------
        -- 2️⃣ Update Test Case Status
        ---------------------------------------------------------
        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        ---------------------------------------------------------
        -- 3️⃣ Return Queue Entry Id + QueueId
        ---------------------------------------------------------
        SELECT  
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;

        THROW; -- return SQL error with full stack
    END CATCH
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAssignedTestCaseStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_UpdateAssignedTestCaseStatus]
(
    @AssignmentTestCaseId INT,
    @TestCaseStatus       VARCHAR(50) = NULL,
    @StartTime            DATETIME = NULL,
    @EndTime              DATETIME = NULL,
    @Duration             FLOAT = NULL,      -- FIXED
    @ErrorMessage         NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Auto calculate duration if Start & End provided AND duration not provided
    IF (@StartTime IS NOT NULL AND @EndTime IS NOT NULL AND (@Duration IS NULL OR @Duration <= 0))
    BEGIN
        SET @Duration = DATEDIFF(SECOND, @StartTime, @EndTime);
    END

    UPDATE aut.AssignedTestCases
    SET 
        TestCaseStatus = ISNULL(@TestCaseStatus, TestCaseStatus),
        StartTime      = ISNULL(@StartTime, StartTime),
        EndTime        = ISNULL(@EndTime, EndTime),
        Duration       = ISNULL(@Duration, Duration),
        ErrorMessage   = ISNULL(@ErrorMessage, ErrorMessage)
    WHERE 
        AssignmentTestCaseId = @AssignmentTestCaseId;

    IF (@@ROWCOUNT > 0)
        SELECT 1 AS Success;
    ELSE
        SELECT 0 AS Success;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationData]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_UpdateAutomationData]
(
    @ID INT,
    @TestContent NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[AutomationData]
    SET TestContent = @TestContent
    WHERE ID = @ID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationDataSections]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateAutomationDataSections] @SectionName NVARCHAR(500) = NULL
	,@SectionID INT
	,@FlowName NVARCHAR(MAX) = NULL
AS
BEGIN
	UPDATE [aut].AutomationDataSections
	SET SectionName = @SectionName
		,FlowName = @FlowName
	WHERE SectionID = @SectionID
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateQueueStatus]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_UpdateQueueStatus]
(
    @QueueId UNIQUEIDENTIFIER,
    @QueueStatus NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.TestCaseExecutionQueue
    SET 
        QueueStatus = @QueueStatus,
        ModifiedDate = GETUTCDATE()   -- auto update modified timestamp
    WHERE 
        QueueId = @QueueId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateUser]
    @UserID INT,
    @UserName       NVARCHAR(100),
    @Password       NVARCHAR(255) = NULL,
    @FirstName      NVARCHAR(100) = NULL,
    @LastName       NVARCHAR(100) = NULL,
    @Email          NVARCHAR(255),
    @Photo          VARBINARY(MAX) = NULL,
    @RoleID         INT,
    @Active         BIT = 1,           
    @TimeZone       INT = NULL,
    @TwoFactor      BIT = 0,          
    @Teams          NVARCHAR(255) = NULL,
    @PhoneNumber    NVARCHAR(20) = NULL,
    @Priority       INT = NULL,
    @Status         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[User]
    SET 
        UserName        = @UserName,
        Password        = COALESCE(@Password, Password),
        FirstName       = @FirstName,
        LastName        = @LastName,
        Email           = @Email,
        Photo           = @Photo,
        RoleID          = @RoleID,
        Active          = @Active,
        TimeZone        = @TimeZone,
        TwoFactorEnabled= @TwoFactor,
        TeamsProjects   = @Teams,
        PhoneNumber     = @PhoneNumber,
        Priority        = @Priority,
        Status          = @Status
    WHERE UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ValidateUserByUsernameAndPassword]    Script Date: 03-12-2025 00:33:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_ValidateUserByUsernameAndPassword]
    @Username NVARCHAR(255),
    @Password NVARCHAR(255)
AS
BEGIN
    SELECT 
        usr.UserID,
        usr.UserName,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
		usr.Active,
        role.RoleName,
		role.RoleID
    FROM 
	[aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
    WHERE usr.UserName = @Username
      AND usr.Password = @Password
	  AND usr.Active=1;

   /*to update last login when user logs in successfully*/
	UPDATE [aut].[User]
	set LastLogin = GETDATE()
	where UserName = @Username
       AND Password = @Password
	  AND Active=1;
END
GO
USE [master]
GO
ALTER DATABASE [MES_AUT] SET  READ_WRITE 
GO
