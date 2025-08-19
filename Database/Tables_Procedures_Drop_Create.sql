USE [OH_PNM_AUTM]
GO
/****** Object:  StoredProcedure [aut].[usp_ValidateUserByEmailAndPassword]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ValidateUserByEmailAndPassword]
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateQueue]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateQueue]
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationDataSections]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateAutomationDataSections]
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateAutomationData]
GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_SetUserActiveStatus]
GO
/****** Object:  StoredProcedure [aut].[usp_SearchTestResults]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_SearchTestResults]
GO
/****** Object:  StoredProcedure [aut].[usp_SearchQueues]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_SearchQueues]
GO
/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_InsertTestScreenshot]
GO
/****** Object:  StoredProcedure [aut].[usp_InsertTestResults]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_InsertTestResults]
GO
/****** Object:  StoredProcedure [aut].[usp_InsertQueue]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_InsertQueue]
GO
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_InsertAutomationDataSection]
GO
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_InsertAutomationData]
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserRoles]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetUserRoles]
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetUserById]
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestResults]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetTestResults]
GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByTestResultId]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetScreenshotsByTestResultId]
GO
/****** Object:  StoredProcedure [aut].[usp_GetQueues]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetQueues]
GO
/****** Object:  StoredProcedure [aut].[usp_GetQueueReports]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetQueueReports]
GO
/****** Object:  StoredProcedure [aut].[usp_GetQueueById]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetQueueById]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationFlowNames]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAutomationFlowNames]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAutomationDataSection]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataByFlowName]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAutomationDataByFlowName]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAutomationData]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAllUsers]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllTestResults]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAllTestResults]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllQueues]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAllQueues]
GO
/****** Object:  StoredProcedure [aut].[usp_get_AutomationFlowNames]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_get_AutomationFlowNames]
GO
/****** Object:  StoredProcedure [aut].[usp_get_AutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_get_AutomationDataSection]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteUser]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestScreenshotById]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteTestScreenshotById]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteQueue]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteQueue]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteAutomationDataSection]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteAutomationData]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_CreateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 10-07-2025 00:02:26 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ChangePassword]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[QueueInfo]') AND type in (N'U'))
ALTER TABLE [aut].[QueueInfo] DROP CONSTRAINT IF EXISTS [FK_QueueInfo_User]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestScreenshots]') AND type in (N'U'))
ALTER TABLE [aut].[TestScreenshots] DROP CONSTRAINT IF EXISTS [DF__TestScree__Taken__18EBB532]
GO
/****** Object:  Table [aut].[TestScreenshots]    Script Date: 10-07-2025 00:02:26 ******/
DROP TABLE IF EXISTS [aut].[TestScreenshots]
GO
/****** Object:  Table [aut].[TestResults]    Script Date: 10-07-2025 00:02:26 ******/
DROP TABLE IF EXISTS [aut].[TestResults]
GO
/****** Object:  Table [aut].[QueueInfo]    Script Date: 10-07-2025 00:02:26 ******/
DROP TABLE IF EXISTS [aut].[QueueInfo]
GO
/****** Object:  Table [aut].[AutomationDataSections]    Script Date: 10-07-2025 00:02:26 ******/
DROP TABLE IF EXISTS [aut].[AutomationDataSections]
GO
/****** Object:  Table [aut].[AutomationData]    Script Date: 10-07-2025 00:02:26 ******/
DROP TABLE IF EXISTS [aut].[AutomationData]
GO
/****** Object:  Table [aut].[AutomationData]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  Table [aut].[AutomationDataSections]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  Table [aut].[QueueInfo]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[QueueInfo](
	[QueueId] [nvarchar](50) NULL,
	[TagName] [nvarchar](150) NULL,
	[EmpId] [nvarchar](150) NULL,
	[QueueName] [nvarchar](150) NULL,
	[QueueDescription] [nvarchar](250) NULL,
	[ProductLine] [nvarchar](150) NULL,
	[QueueStatus] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LibraryName] [nvarchar](max) NULL,
	[ClassName] [nvarchar](max) NULL,
	[MethodName] [nvarchar](max) NULL,
	[UserID] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestResults]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestResults](
	[Name] [nvarchar](150) NULL,
	[ResultStatus] [nvarchar](50) NULL,
	[Duration] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Message] [nvarchar](500) NULL,
	[ClassName] [nvarchar](150) NULL,
	[QueueId] [nvarchar](150) NULL,
	[Id] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestScreenshots]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [aut].[TestScreenshots] ADD  DEFAULT (getutcdate()) FOR [TakenAt]
GO
ALTER TABLE [aut].[QueueInfo]  WITH CHECK ADD  CONSTRAINT [FK_QueueInfo_User] FOREIGN KEY([UserID])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[QueueInfo] CHECK CONSTRAINT [FK_QueueInfo_User]
GO
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_CreateUser]
    @UserName NVARCHAR(100),
    @Password NVARCHAR(255),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255),
    @Photo VARBINARY(MAX),
    @RoleID INT,
	@Active Bit
AS
BEGIN
    INSERT INTO [aut].[User] (UserName, Password, FirstName, LastName, Email, Photo, RoleID,Active)
    VALUES (@UserName, @Password, @FirstName, @LastName, @Email, @Photo, @RoleID,@Active);
    
    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteQueue]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_DeleteQueue]
    @QueueId NVARCHAR(500)
AS
BEGIN
    BEGIN TRY
        -- Delete the queue from QueueInfo table
        DELETE FROM [aut].[QueueInfo]
        WHERE [QueueId] = @QueueId;

        -- Check if any rows were affected (optional)
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Queue with the given QueueId does not exist.', 16, 1);
        END
    END TRY
    BEGIN CATCH
        -- Capture error details
        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT @ErrorMessage = ERROR_MESSAGE(),
               @ErrorSeverity = ERROR_SEVERITY(),
               @ErrorState = ERROR_STATE();

        -- Raise error back to calling application
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestScreenshotById]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_get_AutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_get_AutomationFlowNames]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAllQueues]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAllQueues]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [aut].[QueueInfo] WITH (NOLOCK)
    WHERE QueueStatus = 'New';
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllTestResults]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAllTestResults]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [aut].[TestResults] WITH (NOLOCK)
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 10-07-2025 00:02:26 ******/
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
		role.RoleID
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataByFlowName]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationFlowNames]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetQueueById]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetQueueById]
    @QueueId NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    -- Handle empty string inputs by setting them to NULL
    IF @QueueId = ''
    BEGIN
        SET @QueueId = NULL;
    END

    -- Retrieve the queue by ID only if it is provided
    IF @QueueId IS NOT NULL
    BEGIN
        PRINT 'Fetching queue by ID';

        SELECT 
            [QueueId],
            [TagName],
            [EmpId],
            [QueueName],
            [QueueDescription],
            [ProductLine],
            [QueueStatus],
            [CreatedDate],
            [Id],
            [LibraryName],
            [ClassName],
            [MethodName]
        FROM [aut].[QueueInfo] WITH (NOLOCK)
        WHERE QueueId = @QueueId;
    END
    ELSE
    BEGIN
        PRINT 'Queue ID is required';
    END
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetQueueReports]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetQueueReports]
    @UserId INT,
    @Status NVARCHAR(50) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Page INT = 1,
    @PageSize INT = 0,
    @SortColumn NVARCHAR(250) = 'CreatedDate',
    @SortDirection NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @RoleName NVARCHAR(50);

    -- Validate SortDirection
    SET @SortDirection = 
        CASE UPPER(@SortDirection)
            WHEN 'ASC' THEN 'ASC'
            WHEN 'DESC' THEN 'DESC'
            ELSE 'DESC'
        END;

    -- Validate SortColumn via whitelist
    DECLARE @AllowedColumns TABLE (ColumnName NVARCHAR(100));
    INSERT INTO @AllowedColumns (ColumnName)
    VALUES 
        ('QueueId'), ('TagName'), ('EmpId'), ('QueueName'),
        ('QueueDescription'), ('ProductLine'), ('QueueStatus'),
        ('CreatedDate'), ('Id'), ('LibraryName'), 
        ('ClassName'), ('MethodName');

    IF NOT EXISTS (SELECT 1 FROM @AllowedColumns WHERE ColumnName = @SortColumn)
    BEGIN
        SET @SortColumn = 'CreatedDate'; -- default fallback
    END

    -- Get user's role
    SELECT @RoleName = ur.RoleName
    FROM [aut].[User] u WITH (NOLOCK)
    INNER JOIN [aut].[UserRole] ur WITH (NOLOCK) ON u.RoleID = ur.RoleID
    WHERE u.UserID = @UserId;

    -- Build SQL
    DECLARE @Sql NVARCHAR(MAX) = N'
        SELECT  
            q.QueueId,
            q.TagName,            
            q.QueueName,
            q.QueueDescription,
            q.ProductLine,
            q.QueueStatus,
            q.CreatedDate,
            q.Id,
            q.LibraryName,
            q.ClassName,
            q.MethodName,
            q.UserID,
            u.UserName,
            COUNT(*) OVER() AS TotalCount
        FROM [aut].[QueueInfo] q WITH (NOLOCK)
		LEFT JOIN [aut].[User] u WITH (NOLOCK) ON q.UserID = u.UserID
        WHERE 1 = 1
            AND (@Status IS NULL OR LTRIM(RTRIM(@Status)) = '''' OR QueueStatus = @Status)
            AND (@FromDate IS NULL OR CreatedDate >= @FromDate)
            AND (@ToDate IS NULL OR CreatedDate <= @ToDate)';

    -- Restrict for non-admins
    IF @RoleName != 'Admin'
    BEGIN
        SET @Sql += N'
            AND q.UserID = @UserId';
    END

    -- Append ORDER BY using QUOTENAME to prevent injection
    SET @Sql += N'
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection;

    -- Append pagination if required
    IF @PageSize > 0
    BEGIN
        SET @Sql += N'
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY';

        EXEC sp_executesql @Sql,
            N'@UserId INT, @Status NVARCHAR(50), @FromDate DATETIME, @ToDate DATETIME, @Offset INT, @PageSize INT',
            @UserId, @Status, @FromDate, @ToDate, @Offset, @PageSize;
    END
    ELSE
    BEGIN
        EXEC sp_executesql @Sql,
            N'@UserId INT, @Status NVARCHAR(50), @FromDate DATETIME, @ToDate DATETIME',
            @UserId, @Status, @FromDate, @ToDate;
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetQueues]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetQueues] (
	@QueueId NVARCHAR(150)
	,@QueueStatus NVARCHAR(150)
	)
AS
BEGIN
	IF @QueueId = ''
	BEGIN
		SET @QueueId = NULL
	END

	IF @QueueStatus = ''
	BEGIN
		SET @QueueStatus = NULL
	END

	IF @QueueId IS NOT NULL
		AND @QueueStatus IS NULL
	BEGIN
		PRINT 'Step 1'

		SELECT *
		FROM [aut].[QueueInfo] WITH (NOLOCK)
		WHERE QueueId = @QueueId
	END
	ELSE IF @QueueStatus IS NOT NULL
		AND @QueueId IS NULL
	BEGIN
		PRINT 'Step 2'

		SELECT *
		FROM [aut].[QueueInfo] WITH (NOLOCK)
		WHERE QueueStatus = @QueueStatus
	END
	ELSE IF @QueueId IS NOT NULL
		AND @QueueStatus IS NOT NULL
	BEGIN
		PRINT 'Step 3'

		SELECT *
		FROM [aut].[QueueInfo] WITH (NOLOCK)
		WHERE QueueStatus = @QueueStatus
			AND QueueId = @QueueId
	END
	ELSE IF (@QueueId IS NULL)
		AND (@QueueStatus IS NULL)
	BEGIN
		PRINT 'Step 4'

		SELECT *
		FROM [aut].[QueueInfo] WITH (NOLOCK)
	END
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByTestResultId]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetScreenshotsByTestResultId]
    @TestResultId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        s.ID,
        s.TestResultId,
        s.QueueId,
        s.Caption,
        s.Screenshot,
        s.TakenAt,
        q.[UserID],
        u.[UserName]
    FROM [aut].[TestScreenshots] s
    INNER JOIN [aut].[QueueInfo] q ON s.QueueId = q.QueueId
    INNER JOIN [aut].[Users] u ON q.UserID = u.UserID
    WHERE s.TestResultId = @TestResultId;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestResults]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetTestResults] @QueueId NVARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT *
	FROM [aut].[TestResults] WITH (NOLOCK)
	WHERE QueueId = @QueueId
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 10-07-2025 00:02:26 ******/
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
		role.RoleID
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
    WHERE usr.UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserRoles]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationDataSection]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_InsertQueue]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [aut].[usp_InsertQueue] (
	@CreatedDate AS DATETIME = NULL
	,@QueueId AS NVARCHAR(50) = NULL
	,@TagName AS NVARCHAR(150) = NULL
	,@EmpId AS NVARCHAR(50) = NULL
	,@QueueName AS NVARCHAR(150) = NULL
	,@QueueDescription AS NVARCHAR(500) = NULL
	,@ProductLine AS NVARCHAR(150) = NULL
	,@QueueStatus AS NVARCHAR(150) = NULL
	,@LibraryName AS NVARCHAR(MAX) = NULL
	,@MethodName AS NVARCHAR(MAX) = NULL
	,@ClassName AS NVARCHAR(MAX) = NULL
	,@UserID AS INT 
	)
AS
BEGIN TRANSACTION

BEGIN TRY
	-- insert
	INSERT INTO [aut].[QueueInfo] (
		[QueueId]
		,[TagName]
		,[EmpId]
		,[QueueName]
		,[QueueDescription]
		,[ProductLine]
		,[QueueStatus]
		,[CreatedDate]
		,[LibraryName]
		,[MethodName]
		,[ClassName]
		,[UserID]
		)
	VALUES (
		@QueueId
		,@TagName
		,@EmpId
		,@QueueName
		,@QueueDescription
		,@ProductLine
		,@QueueStatus
		,@CreatedDate
		,@LibraryName
		,@MethodName
		,@ClassName
		,@UserID
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
/****** Object:  StoredProcedure [aut].[usp_InsertTestResults]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [aut].[usp_InsertTestResults] (
	@Name AS NVARCHAR(150) = NULL
	,@ResultStatus AS NVARCHAR(50) = NULL
	,@Duration AS NVARCHAR(50) = NULL
	,@StartTime AS DATETIME = NULL
	,@EndTime AS DATETIME = NULL
	,@Message AS NVARCHAR(500) = NULL
	,@ClassName AS NVARCHAR(150) = NULL
	,@QueueId AS NVARCHAR(150) = NULL
	)
AS
BEGIN TRANSACTION

BEGIN TRY

	-- insert
	INSERT INTO [aut].[TestResults] (
		[Name]
		,[ResultStatus]
		,[Duration]
		,[StartTime]
		,[EndTime]
		,[Message]
		,[ClassName]
		,[QueueId]
		)
	VALUES (
		@Name
		,@ResultStatus
		,@Duration
		,@StartTime
		,@EndTime
		,@Message
		,@ClassName
		,@QueueId
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
/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
/****** Object:  StoredProcedure [aut].[usp_SearchQueues]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_SearchQueues]
    @UserId INT,
    @Page INT = 1,
    @PageSize INT = 0,
    @SortColumn NVARCHAR(MAX) = 'Id',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;

    -- Validate and sanitize SortDirection
    SET @SortDirection = 
        CASE UPPER(@SortDirection)
            WHEN 'ASC' THEN 'ASC'
            WHEN 'DESC' THEN 'DESC'
            ELSE 'ASC'
        END;

    -- Column whitelist for safe dynamic ORDER BY
    DECLARE @AllowedColumns TABLE (ColumnName NVARCHAR(100));
    INSERT INTO @AllowedColumns (ColumnName)
    VALUES ('QueueId'), ('TagName'), ('EmpId'), ('QueueName'), 
           ('QueueDescription'), ('ProductLine'), ('QueueStatus'), 
           ('CreatedDate'), ('Id'), ('LibraryName'), 
           ('ClassName'), ('MethodName'), ('UserID'), ('UserName');

    IF NOT EXISTS (SELECT 1 FROM @AllowedColumns WHERE ColumnName = @SortColumn)
    BEGIN
        SET @SortColumn = 'Id'; -- default fallback
    END

    DECLARE @Sql NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = '';
    DECLARE @RoleName NVARCHAR(50);

    -- Get user's role
    SELECT @RoleName = ur.RoleName
    FROM [aut].[User] u WITH (NOLOCK)
    INNER JOIN [aut].[UserRole] ur WITH (NOLOCK) ON u.RoleID = ur.RoleID
    WHERE u.UserID = @UserId;

    IF @RoleName != 'Admin'
    BEGIN
        SET @WhereClause = 'WHERE q.UserID = @UserId';
    END

    -- Build SQL query dynamically
    SET @Sql = '
        SELECT 
            q.QueueId,
            q.TagName,
            q.EmpId,
            q.QueueName,
            q.QueueDescription,
            q.ProductLine,
            q.QueueStatus,
            q.CreatedDate,
            q.Id,
            q.LibraryName,
            q.ClassName,
            q.MethodName,
            q.UserID,
            u.UserName,
            COUNT(*) OVER() AS TotalCount
        FROM [aut].[QueueInfo] q WITH (NOLOCK)
        LEFT JOIN [aut].[User] u WITH (NOLOCK) ON q.UserID = u.UserID
        ' + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection;

    -- Append pagination if required
    IF @PageSize > 0
    BEGIN
        SET @Sql += '
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;';
    END;

    -- Execute dynamic SQL
    IF @PageSize > 0
    BEGIN
        EXEC sp_executesql @Sql,
            N'@UserId INT, @Offset INT, @PageSize INT',
            @UserId = @UserId, @Offset = @Offset, @PageSize = @PageSize;
    END
    ELSE
    BEGIN
        EXEC sp_executesql @Sql,
            N'@UserId INT',
            @UserId = @UserId;
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_SearchTestResults]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_SearchTestResults]
    @UserId INT,
    @Page INT = 1,
    @PageSize INT = 0,
    @SortColumn NVARCHAR(100) = 'Id',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@Page - 1) * @PageSize;
    DECLARE @RoleName NVARCHAR(50);

    -- Get user's role
    SELECT @RoleName = ur.RoleName
    FROM [aut].[User] u WITH (NOLOCK)
    INNER JOIN [aut].[UserRole] ur WITH (NOLOCK) ON u.RoleID = ur.RoleID
    WHERE u.UserID = @UserId;

    -- Sanitize sort direction
    SET @SortDirection = 
        CASE UPPER(@SortDirection)
            WHEN 'ASC' THEN 'ASC'
            WHEN 'DESC' THEN 'DESC'
            ELSE 'ASC'
        END;

    -- Column whitelist
    DECLARE @AllowedColumns TABLE (ColumnName NVARCHAR(100));
    INSERT INTO @AllowedColumns (ColumnName)
    VALUES ('Id'), ('Name'), ('ResultStatus'), ('Duration'), 
           ('StartTime'), ('EndTime'), ('Message'), 
           ('ClassName'), ('QueueId');

    IF NOT EXISTS (SELECT 1 FROM @AllowedColumns WHERE ColumnName = @SortColumn)
    BEGIN
        SET @SortColumn = 'Id'; -- fallback
    END

    -- Build base query with JOIN to QueueInfo
    DECLARE @Sql NVARCHAR(MAX) = N'
        SELECT 
            tr.[Id],
            tr.[Name],
            tr.[ResultStatus],
            tr.[Duration],
            tr.[StartTime],
            tr.[EndTime],
            tr.[Message],
            tr.[ClassName],
            tr.[QueueId],
            COUNT(*) OVER() AS TotalCount
        FROM [aut].[TestResults] tr WITH (NOLOCK)
        INNER JOIN [aut].[QueueInfo] q WITH (NOLOCK) ON tr.QueueId = q.QueueId
        WHERE 1 = 1';

    -- Restrict to only user's results if not admin
    IF @RoleName != 'Admin'
    BEGIN
        SET @Sql += N' AND q.UserId = @UserId';
    END

    -- Add dynamic ORDER BY
    SET @Sql += N'
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection;

    -- Pagination or not
    IF @PageSize = 0
    BEGIN
        EXEC sp_executesql @Sql,
            N'@UserId INT',
            @UserId;
    END
    ELSE
    BEGIN
        SET @Sql += N'
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY';

        EXEC sp_executesql @Sql,
            N'@UserId INT, @Offset INT, @PageSize INT',
            @UserId, @Offset, @PageSize;
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationData]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationDataSections]    Script Date: 10-07-2025 00:02:26 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateQueue]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateQueue] @QueueId NVARCHAR(150)
	,@QueueStatus NVARCHAR(150)
AS
BEGIN
	UPDATE [aut].QueueInfo
	SET QueueStatus = @QueueStatus
	WHERE QueueId = @QueueId
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateUser]
    @UserID INT,
    @UserName NVARCHAR(100),    
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(255),
    @Photo VARBINARY(MAX),
    @RoleID INT	
AS
BEGIN
    UPDATE [aut].[User]
    SET 
        UserName = @UserName,        
        FirstName = @FirstName,
        LastName = @LastName,
        Email = @Email,
        Photo = @Photo,
        RoleID = @RoleID		
    WHERE UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ValidateUserByEmailAndPassword]    Script Date: 10-07-2025 00:02:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_ValidateUserByEmailAndPassword]
    @Email NVARCHAR(255),
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
    WHERE usr.Email = @Email
      AND usr.Password = @Password
	  AND usr.Active=1;
END
GO
