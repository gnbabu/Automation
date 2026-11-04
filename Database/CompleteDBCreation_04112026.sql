
/****** Object:  User [automation_user]    Script Date: 04-11-2026 21:35:15 ******/
CREATE USER [automation_user] FOR LOGIN [automation_user] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [automation_user]
GO
ALTER ROLE [db_datareader] ADD MEMBER [automation_user]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [automation_user]
GO
/****** Object:  Schema [aut]    Script Date: 04-11-2026 21:35:15 ******/
CREATE SCHEMA [aut]
GO
/****** Object:  UserDefinedTableType [aut].[AssignmentTestCaseIdList]    Script Date: 04-11-2026 21:35:15 ******/
CREATE TYPE [aut].[AssignmentTestCaseIdList] AS TABLE(
	[AssignmentTestCaseId] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [aut].[TestCaseType]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  UserDefinedTableType [aut].[TestScreenshotType]    Script Date: 04-11-2026 21:35:15 ******/
CREATE TYPE [aut].[TestScreenshotType] AS TABLE(
	[AssignmentTestCaseId] [int] NULL,
	[Caption] [nvarchar](max) NULL,
	[Screenshot] [varbinary](max) NULL,
	[TakenAt] [datetime] NULL
)
GO
/****** Object:  Table [aut].[AssignedTestCases]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Table [aut].[AuditLog]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AuditLog](
	[AuditLogId] [int] IDENTITY(1,1) NOT NULL,
	[EntityType] [nvarchar](50) NOT NULL,
	[EntityId] [int] NULL,
	[EntityName] [nvarchar](200) NOT NULL,
	[Action] [nvarchar](30) NOT NULL,
	[ActorUserId] [int] NULL,
	[ActorUserName] [nvarchar](100) NOT NULL,
	[Details] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED 
(
	[AuditLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[AutomationData]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AutomationData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SectionID] [int] NULL,
	[TestContent] [varchar](max) NULL,
	[UserID] [int] NULL,
	[EnvironmentId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[AutomationDataSections]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[AutomationDataSections](
	[SectionID] [int] IDENTITY(1,1) NOT NULL,
	[SectionName] [nvarchar](250) NULL,
	[FlowName] [nvarchar](250) NULL,
	[IsFromModel] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[Environment]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[Environment](
	[EnvironmentId] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[ModifiedBy] [int] NULL,
	[EnvironmentUrl] [nvarchar](500) NULL,
	[RequiresAuthentication] [bit] NOT NULL,
	[EnableSso] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[LoginUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[LoginUser](
	[LoginUserId] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentId] [int] NOT NULL,
	[PortalUserId] [int] NULL,
	[UserRole] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[EncryptedPassword] [nvarchar](500) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [int] NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedOn] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[LoginUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[Notification]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[Notification](
	[NotificationId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[NotificationType] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[LinkUrl] [nvarchar](300) NULL,
	[SourceType] [nvarchar](30) NOT NULL,
	[SourceId] [int] NULL,
	[IsRead] [bit] NOT NULL,
	[ReadOn] [datetime2](7) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[PriorityStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Table [aut].[RecurringSchedule]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[RecurringSchedule](
	[RecurringScheduleId] [int] IDENTITY(1,1) NOT NULL,
	[AssignmentId] [int] NOT NULL,
	[RecurrenceType] [nvarchar](20) NOT NULL,
	[DaysOfWeek] [nvarchar](20) NULL,
	[DayOfMonth] [int] NULL,
	[TimeOfDay] [time](0) NOT NULL,
	[Browser] [nvarchar](100) NOT NULL,
	[LoginUserId] [int] NULL,
	[IsActive] [bit] NOT NULL,
	[PausedReason] [nvarchar](300) NULL,
	[EndDate] [datetime2](7) NULL,
	[NextRunDate] [datetime2](7) NOT NULL,
	[LastRunDate] [datetime2](7) NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[RunCount] [int] NOT NULL,
 CONSTRAINT [PK_RecurringSchedule] PRIMARY KEY CLUSTERED 
(
	[RecurringScheduleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[RecurringScheduleRunHistory]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[RecurringScheduleRunHistory](
	[RunHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[RecurringScheduleId] [int] NOT NULL,
	[RunDate] [datetime2](7) NOT NULL,
	[Result] [nvarchar](30) NOT NULL,
	[Detail] [nvarchar](300) NULL,
	[TestCasesQueuedCount] [int] NOT NULL,
	[ResolutionStatus] [nvarchar](20) NOT NULL,
	[PassedCount] [int] NULL,
	[FailedCount] [int] NULL,
	[SkippedCount] [int] NULL,
	[ResolvedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_RecurringScheduleRunHistory] PRIMARY KEY CLUSTERED 
(
	[RunHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[RecurringScheduleRunHistoryTestCase]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[RecurringScheduleRunHistoryTestCase](
	[RunHistoryId] [bigint] NOT NULL,
	[AssignmentTestCaseId] [int] NOT NULL,
 CONSTRAINT [PK_RecurringScheduleRunHistoryTestCase] PRIMARY KEY CLUSTERED 
(
	[RunHistoryId] ASC,
	[AssignmentTestCaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[Release]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[Release](
	[ReleaseId] [int] IDENTITY(1,1) NOT NULL,
	[ReleaseName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ReleaseLifecycle] [nvarchar](30) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SignOffStatus] [nvarchar](20) NOT NULL,
	[SignedOffBy] [nvarchar](100) NULL,
	[SignedOffOn] [datetime2](7) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedOn] [datetime2](7) NULL,
	[Version] [nvarchar](50) NULL,
	[EnvironmentId] [int] NULL,
	[ReleaseFolderPath] [nvarchar](500) NULL,
	[ModifiedBy] [nvarchar](100) NULL,
	[ActivatedBy] [nvarchar](100) NULL,
	[ActivatedOn] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[ReleaseNotification]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[ReleaseNotification](
	[ReleaseNotificationId] [int] IDENTITY(1,1) NOT NULL,
	[ReleaseId] [int] NOT NULL,
	[NotificationType] [nvarchar](50) NOT NULL,
	[RecipientUserId] [int] NULL,
	[RecipientEmail] [nvarchar](255) NULL,
	[Status] [nvarchar](30) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[SentOn] [datetime2](7) NULL,
 CONSTRAINT [PK_ReleaseNotification] PRIMARY KEY CLUSTERED 
(
	[ReleaseNotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[ReleaseSignOff]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[ReleaseSignOff](
	[ReleaseSignOffId] [int] IDENTITY(1,1) NOT NULL,
	[ReleaseId] [int] NOT NULL,
	[SignOffStatus] [nvarchar](20) NOT NULL,
	[SignOffBy] [nvarchar](100) NULL,
	[SignOffOn] [datetime2](7) NULL,
	[Comments] [nvarchar](1000) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ReleaseSignOff] PRIMARY KEY CLUSTERED 
(
	[ReleaseSignOffId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseAssignment]    Script Date: 04-11-2026 21:35:15 ******/
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
	[ReleaseId] [int] NOT NULL,
	[EnvironmentId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[AssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseData]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseData](
	[TestCaseDataId] [int] IDENTITY(1,1) NOT NULL,
	[TestCaseKeyId] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[EnvironmentId] [int] NOT NULL,
	[FlowName] [nvarchar](250) NOT NULL,
	[DataJson] [nvarchar](max) NOT NULL,
	[Version] [timestamp] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_TestCaseData] PRIMARY KEY CLUSTERED 
(
	[TestCaseDataId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_TestCaseData] UNIQUE NONCLUSTERED 
(
	[TestCaseKeyId] ASC,
	[UserID] ASC,
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseData_SectionRows_Backup]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseData_SectionRows_Backup](
	[TestCaseDataId] [int] IDENTITY(1,1) NOT NULL,
	[LibraryName] [nvarchar](255) NOT NULL,
	[ClassName] [nvarchar](255) NOT NULL,
	[MethodName] [nvarchar](255) NOT NULL,
	[SectionID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[EnvironmentId] [int] NOT NULL,
	[TestContent] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_TestCaseData_SectionRows_Backup] PRIMARY KEY CLUSTERED 
(
	[TestCaseDataId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseExecutionLogs]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseExecutionLogs](
	[LogId] [bigint] IDENTITY(1,1) NOT NULL,
	[AssignmentId] [int] NOT NULL,
	[AssignmentTestCaseId] [int] NOT NULL,
	[StepName] [nvarchar](255) NOT NULL,
	[LogMessage] [nvarchar](max) NOT NULL,
	[LogLevel] [nvarchar](20) NOT NULL,
	[ExecutionStatus] [nvarchar](50) NOT NULL,
	[ScreenshotId] [int] NULL,
	[ErrorStackTrace] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestCaseExecutionQueue]    Script Date: 04-11-2026 21:35:15 ******/
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
	[LoginUserId] [int] NULL,
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
/****** Object:  Table [aut].[TestCaseKey]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseKey](
	[TestCaseKeyId] [int] IDENTITY(1,1) NOT NULL,
	[LibraryName] [nvarchar](255) NOT NULL,
	[ClassName] [nvarchar](255) NOT NULL,
	[MethodName] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_TestCaseKey] PRIMARY KEY CLUSTERED 
(
	[TestCaseKeyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UX_TestCaseKey] UNIQUE NONCLUSTERED 
(
	[LibraryName] ASC,
	[ClassName] ASC,
	[MethodName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestExecutionNotification]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestExecutionNotification](
	[TestExecutionNotificationId] [int] IDENTITY(1,1) NOT NULL,
	[AssignmentTestCaseId] [int] NOT NULL,
	[NotificationType] [nvarchar](50) NOT NULL,
	[RecipientUserId] [int] NULL,
	[RecipientEmail] [nvarchar](255) NULL,
	[Status] [nvarchar](30) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[SentOn] [datetime2](7) NULL,
 CONSTRAINT [PK_TestExecutionNotification] PRIMARY KEY CLUSTERED 
(
	[TestExecutionNotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[TestScreenshots]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Table [aut].[TimeZone]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Table [aut].[User]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
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
	[ResetPasswordToken] [nvarchar](200) NULL,
	[ResetPasswordExpiry] [datetime] NULL,
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
/****** Object:  Table [aut].[UserRole]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Table [aut].[UserStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  Index [IX_AuditLog_ActorUserId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_ActorUserId] ON [aut].[AuditLog]
(
	[ActorUserId] ASC,
	[CreatedOn] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditLog_CreatedOn]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_CreatedOn] ON [aut].[AuditLog]
(
	[CreatedOn] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditLog_EntityType_EntityId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_AuditLog_EntityType_EntityId] ON [aut].[AuditLog]
(
	[EntityType] ASC,
	[EntityId] ASC,
	[CreatedOn] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UXEnvironmentName]    Script Date: 04-11-2026 21:35:15 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UXEnvironmentName] ON [aut].[Environment]
(
	[EnvironmentName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notification_UserId_IsRead_CreatedOn]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_Notification_UserId_IsRead_CreatedOn] ON [aut].[Notification]
(
	[UserId] ASC,
	[IsRead] ASC,
	[CreatedOn] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RecurringSchedule_NextRunDate]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_RecurringSchedule_NextRunDate] ON [aut].[RecurringSchedule]
(
	[IsActive] ASC,
	[NextRunDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RecurringScheduleRunHistory_Pending]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_RecurringScheduleRunHistory_Pending] ON [aut].[RecurringScheduleRunHistory]
(
	[ResolutionStatus] ASC
)
WHERE ([ResolutionStatus]='Pending')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RecurringScheduleRunHistory_Schedule]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_RecurringScheduleRunHistory_Schedule] ON [aut].[RecurringScheduleRunHistory]
(
	[RecurringScheduleId] ASC,
	[RunDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Release_EnvironmentId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_Release_EnvironmentId] ON [aut].[Release]
(
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Release_Name_Version_Env]    Script Date: 04-11-2026 21:35:15 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Release_Name_Version_Env] ON [aut].[Release]
(
	[ReleaseName] ASC,
	[Version] ASC,
	[EnvironmentId] ASC
)
WHERE ([Version] IS NOT NULL AND [EnvironmentId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Release_ReleaseName]    Script Date: 04-11-2026 21:35:15 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Release_ReleaseName] ON [aut].[Release]
(
	[ReleaseName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReleaseNotification_ReleaseId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_ReleaseNotification_ReleaseId] ON [aut].[ReleaseNotification]
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_ReleaseSignOff_ReleaseId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_ReleaseSignOff_ReleaseId] ON [aut].[ReleaseSignOff]
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TestCaseAssignment_ReleaseId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_TestCaseAssignment_ReleaseId] ON [aut].[TestCaseAssignment]
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_TestCaseData_Key]    Script Date: 04-11-2026 21:35:15 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_TestCaseData_Key] ON [aut].[TestCaseData_SectionRows_Backup]
(
	[LibraryName] ASC,
	[ClassName] ASC,
	[MethodName] ASC,
	[SectionID] ASC,
	[UserID] ASC,
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TestExecutionNotification_AssignmentTestCaseId]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_TestExecutionNotification_AssignmentTestCaseId] ON [aut].[TestExecutionNotification]
(
	[AssignmentTestCaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_User_ResetPasswordToken]    Script Date: 04-11-2026 21:35:15 ******/
CREATE NONCLUSTERED INDEX [IX_User_ResetPasswordToken] ON [aut].[User]
(
	[ResetPasswordToken] ASC
)
WHERE ([ResetPasswordToken] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [aut].[AuditLog] ADD  CONSTRAINT [DF_AuditLog_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[AutomationDataSections] ADD  CONSTRAINT [DF_AutomationDataSections_IsFromModel]  DEFAULT ((0)) FOR [IsFromModel]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((1)) FOR [RequiresAuthentication]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((0)) FOR [EnableSso]
GO
ALTER TABLE [aut].[LoginUser] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[LoginUser] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[Notification] ADD  CONSTRAINT [DF_Notification_IsRead]  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [aut].[Notification] ADD  CONSTRAINT [DF_Notification_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[RecurringSchedule] ADD  CONSTRAINT [DF_RecurringSchedule_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[RecurringSchedule] ADD  CONSTRAINT [DF_RecurringSchedule_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[RecurringSchedule] ADD  CONSTRAINT [DF_RecurringSchedule_RunCount]  DEFAULT ((0)) FOR [RunCount]
GO
ALTER TABLE [aut].[RecurringScheduleRunHistory] ADD  CONSTRAINT [DF_RecurringScheduleRunHistory_RunDate]  DEFAULT (sysdatetime()) FOR [RunDate]
GO
ALTER TABLE [aut].[RecurringScheduleRunHistory] ADD  CONSTRAINT [DF_RecurringScheduleRunHistory_QueuedCount]  DEFAULT ((0)) FOR [TestCasesQueuedCount]
GO
ALTER TABLE [aut].[RecurringScheduleRunHistory] ADD  CONSTRAINT [DF_RecurringScheduleRunHistory_ResolutionStatus]  DEFAULT ('NotApplicable') FOR [ResolutionStatus]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Draft') FOR [ReleaseLifecycle]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Pending') FOR [SignOffStatus]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[ReleaseNotification] ADD  CONSTRAINT [DF_ReleaseNotification_Status]  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [aut].[ReleaseNotification] ADD  CONSTRAINT [DF_ReleaseNotification_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[ReleaseSignOff] ADD  CONSTRAINT [DF_ReleaseSignOff_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[TestCaseData] ADD  CONSTRAINT [DF_TestCaseData_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[TestCaseData_SectionRows_Backup] ADD  CONSTRAINT [DF_TestCaseData_SectionRows_Backup_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[TestCaseExecutionLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [aut].[TestCaseExecutionQueue] ADD  DEFAULT (newid()) FOR [QueueId]
GO
ALTER TABLE [aut].[TestCaseKey] ADD  CONSTRAINT [DF_TestCaseKey_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[TestExecutionNotification] ADD  CONSTRAINT [DF_TestExecutionNotification_Status]  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [aut].[TestExecutionNotification] ADD  CONSTRAINT [DF_TestExecutionNotification_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
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
ALTER TABLE [aut].[AutomationData]  WITH CHECK ADD  CONSTRAINT [FK_AutomationData_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[AutomationData] CHECK CONSTRAINT [FK_AutomationData_Environment]
GO
ALTER TABLE [aut].[Environment]  WITH CHECK ADD  CONSTRAINT [FK_Environment_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[Environment] CHECK CONSTRAINT [FK_Environment_CreatedBy]
GO
ALTER TABLE [aut].[Environment]  WITH CHECK ADD  CONSTRAINT [FK_Environment_ModifiedBy] FOREIGN KEY([ModifiedBy])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[Environment] CHECK CONSTRAINT [FK_Environment_ModifiedBy]
GO
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_CreatedBy]
GO
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_Environment]
GO
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_ModifiedBy] FOREIGN KEY([ModifiedBy])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_ModifiedBy]
GO
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_PortalUser] FOREIGN KEY([PortalUserId])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_PortalUser]
GO
ALTER TABLE [aut].[Notification]  WITH CHECK ADD  CONSTRAINT [FK_Notification_User] FOREIGN KEY([UserId])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[Notification] CHECK CONSTRAINT [FK_Notification_User]
GO
ALTER TABLE [aut].[RecurringSchedule]  WITH CHECK ADD  CONSTRAINT [FK_RecurringSchedule_Assignment] FOREIGN KEY([AssignmentId])
REFERENCES [aut].[TestCaseAssignment] ([AssignmentId])
GO
ALTER TABLE [aut].[RecurringSchedule] CHECK CONSTRAINT [FK_RecurringSchedule_Assignment]
GO
ALTER TABLE [aut].[RecurringScheduleRunHistory]  WITH CHECK ADD  CONSTRAINT [FK_RecurringScheduleRunHistory_Schedule] FOREIGN KEY([RecurringScheduleId])
REFERENCES [aut].[RecurringSchedule] ([RecurringScheduleId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[RecurringScheduleRunHistory] CHECK CONSTRAINT [FK_RecurringScheduleRunHistory_Schedule]
GO
ALTER TABLE [aut].[RecurringScheduleRunHistoryTestCase]  WITH CHECK ADD  CONSTRAINT [FK_RecurringScheduleRunHistoryTestCase_History] FOREIGN KEY([RunHistoryId])
REFERENCES [aut].[RecurringScheduleRunHistory] ([RunHistoryId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[RecurringScheduleRunHistoryTestCase] CHECK CONSTRAINT [FK_RecurringScheduleRunHistoryTestCase_History]
GO
ALTER TABLE [aut].[Release]  WITH CHECK ADD  CONSTRAINT [FK_Release_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[Release] CHECK CONSTRAINT [FK_Release_Environment]
GO
ALTER TABLE [aut].[ReleaseNotification]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseNotification_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[ReleaseNotification] CHECK CONSTRAINT [FK_ReleaseNotification_Release]
GO
ALTER TABLE [aut].[ReleaseNotification]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseNotification_User] FOREIGN KEY([RecipientUserId])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[ReleaseNotification] CHECK CONSTRAINT [FK_ReleaseNotification_User]
GO
ALTER TABLE [aut].[ReleaseSignOff]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseSignOff_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
ON DELETE CASCADE
GO
ALTER TABLE [aut].[ReleaseSignOff] CHECK CONSTRAINT [FK_ReleaseSignOff_Release]
GO
ALTER TABLE [aut].[TestCaseAssignment]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseAssignment_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[TestCaseAssignment] CHECK CONSTRAINT [FK_TestCaseAssignment_Environment]
GO
ALTER TABLE [aut].[TestCaseAssignment]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseAssignment_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
GO
ALTER TABLE [aut].[TestCaseAssignment] CHECK CONSTRAINT [FK_TestCaseAssignment_Release]
GO
ALTER TABLE [aut].[TestCaseData]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseData_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[TestCaseData] CHECK CONSTRAINT [FK_TestCaseData_Environment]
GO
ALTER TABLE [aut].[TestCaseData]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseData_TestCaseKey] FOREIGN KEY([TestCaseKeyId])
REFERENCES [aut].[TestCaseKey] ([TestCaseKeyId])
GO
ALTER TABLE [aut].[TestCaseData] CHECK CONSTRAINT [FK_TestCaseData_TestCaseKey]
GO
ALTER TABLE [aut].[TestCaseData]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseData_User] FOREIGN KEY([UserID])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[TestCaseData] CHECK CONSTRAINT [FK_TestCaseData_User]
GO
ALTER TABLE [aut].[TestCaseData_SectionRows_Backup]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseData_SectionRows_Backup_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
ALTER TABLE [aut].[TestCaseData_SectionRows_Backup] CHECK CONSTRAINT [FK_TestCaseData_SectionRows_Backup_Environment]
GO
ALTER TABLE [aut].[TestCaseData_SectionRows_Backup]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseData_SectionRows_Backup_User] FOREIGN KEY([UserID])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[TestCaseData_SectionRows_Backup] CHECK CONSTRAINT [FK_TestCaseData_SectionRows_Backup_User]
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
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_LoginUser] FOREIGN KEY([LoginUserId])
REFERENCES [aut].[LoginUser] ([LoginUserId])
GO
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_LoginUser]
GO
ALTER TABLE [aut].[TestExecutionNotification]  WITH CHECK ADD  CONSTRAINT [FK_TestExecutionNotification_AssignedTestCases] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
GO
ALTER TABLE [aut].[TestExecutionNotification] CHECK CONSTRAINT [FK_TestExecutionNotification_AssignedTestCases]
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
ALTER TABLE [aut].[TestCaseData]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseData_DataJson] CHECK  ((isjson([DataJson])=(1)))
GO
ALTER TABLE [aut].[TestCaseData] CHECK CONSTRAINT [CK_TestCaseData_DataJson]
GO
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_Consistency] CHECK  (([LogLevel]='Fail' AND [ExecutionStatus]='Failed' OR [LogLevel]='Warning' AND [ExecutionStatus]='Running' OR [LogLevel]='Info' AND [ExecutionStatus]='Running' OR [LogLevel]='Pass' AND ([ExecutionStatus]='Passed' OR [ExecutionStatus]='Running')))
GO
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_Consistency]
GO
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_ExecutionStatus] CHECK  (([ExecutionStatus]='Failed' OR [ExecutionStatus]='Passed' OR [ExecutionStatus]='Running'))
GO
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_ExecutionStatus]
GO
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_LogLevel] CHECK  (([LogLevel]='Fail' OR [LogLevel]='Warning' OR [LogLevel]='Pass' OR [LogLevel]='Info'))
GO
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_LogLevel]
GO
/****** Object:  StoredProcedure [aut].[usp_ActivateRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_ActivateRelease]
(
    @ReleaseId    INT,
    @ActivatedBy  NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EnvironmentId INT, @FolderPath NVARCHAR(500), @EnvActive BIT, @CurrentLifecycle NVARCHAR(50), @LifecycleForMessage NVARCHAR(50);

    SELECT
        @EnvironmentId = EnvironmentId,
        @FolderPath = ReleaseFolderPath,
        @CurrentLifecycle = ReleaseLifecycle
    FROM aut.[Release] WHERE ReleaseId = @ReleaseId;

    IF @EnvironmentId IS NULL
    BEGIN
        RAISERROR('Cannot activate: release has no environment.', 16, 1); RETURN;
    END

    -- Guard: only a Draft release can be activated. Without this, calling activate on an
    -- already-Active/Completed/Rejected release silently re-stamped ActivatedOn/By and let
    -- the controller re-send a full notification batch every time.
    IF @CurrentLifecycle IS NULL OR @CurrentLifecycle <> 'Draft'
    BEGIN
        SET @LifecycleForMessage = ISNULL(@CurrentLifecycle, N'in an unknown lifecycle state');
        RAISERROR('Cannot activate: release is already %s.', 16, 1, @LifecycleForMessage); RETURN;
    END

    SELECT @EnvActive = IsActive FROM aut.[Environment] WHERE EnvironmentId = @EnvironmentId;
    IF ISNULL(@EnvActive, 0) = 0
    BEGIN
        RAISERROR('Cannot activate: environment is not active.', 16, 1); RETURN;
    END

    IF @FolderPath IS NULL OR LTRIM(RTRIM(@FolderPath)) = ''
    BEGIN
        RAISERROR('Cannot activate: release folder path is not set.', 16, 1); RETURN;
    END

    UPDATE aut.[Release]
    SET ReleaseLifecycle = 'Active',
        IsActive         = 1,
        ActivatedBy      = @ActivatedBy,
        ActivatedOn      = SYSDATETIME(),
        ModifiedBy       = @ActivatedBy,
        ModifiedOn       = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_AddTestCaseExecutionLog]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_AddTestCaseExecutionLog]
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @StepName NVARCHAR(255),
    @LogMessage NVARCHAR(MAX),
    @LogLevel NVARCHAR(20),
    @ExecutionStatus NVARCHAR(50),
    @ScreenshotId INT = NULL,
    @ErrorStackTrace NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.TestCaseExecutionLogs
    (
        AssignmentId,
        AssignmentTestCaseId,
        StepName,
        LogMessage,
        LogLevel,
        ExecutionStatus,
        ScreenshotId,
        ErrorStackTrace
    )
    VALUES
    (
        @AssignmentId,
        @AssignmentTestCaseId,
        @StepName,
        @LogMessage,
        @LogLevel,
        @ExecutionStatus,
        @ScreenshotId,
        @ErrorStackTrace
    );
END;
GO
/****** Object:  StoredProcedure [aut].[usp_AuditLog_GetDistinctEntityTypes]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Powers the Entity Type filter dropdown on the Activity Log screen without hard-coding
-- the list on the frontend - reflects exactly what's actually been logged so far.
CREATE PROCEDURE [aut].[usp_AuditLog_GetDistinctEntityTypes]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT [EntityType] FROM aut.[AuditLog] ORDER BY [EntityType];
END

GO
/****** Object:  StoredProcedure [aut].[usp_AuditLog_GetPaged]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_AuditLog_GetPaged]
    @EntityType  NVARCHAR(50)  = NULL,
    @EntityId    INT           = NULL,
    @ActorUserId INT           = NULL,
    @Action      NVARCHAR(30)  = NULL,
    @FromDate    DATETIME2(7)  = NULL,
    @ToDate      DATETIME2(7)  = NULL,
    @PageNumber  INT           = 1,
    @PageSize    INT           = 25
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 25;

    SELECT
        [AuditLogId],
        [EntityType],
        [EntityId],
        [EntityName],
        [Action],
        [ActorUserId],
        [ActorUserName],
        [Details],
        [CreatedOn],
        COUNT(*) OVER() AS TotalCount
    FROM aut.[AuditLog]
    WHERE (@EntityType IS NULL OR [EntityType] = @EntityType)
      AND (@EntityId IS NULL OR [EntityId] = @EntityId)
      AND (@ActorUserId IS NULL OR [ActorUserId] = @ActorUserId)
      AND (@Action IS NULL OR [Action] = @Action)
      AND (@FromDate IS NULL OR [CreatedOn] >= @FromDate)
      AND (@ToDate IS NULL OR [CreatedOn] <= @ToDate)
    ORDER BY [CreatedOn] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END

GO
/****** Object:  StoredProcedure [aut].[usp_AuditLog_Insert]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_AuditLog_Insert]
    @EntityType    NVARCHAR(50),
    @EntityId      INT = NULL,
    @EntityName    NVARCHAR(200),
    @Action        NVARCHAR(30),
    @ActorUserId   INT = NULL,
    @ActorUserName NVARCHAR(100),
    @Details       NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.[AuditLog]
        ([EntityType], [EntityId], [EntityName], [Action], [ActorUserId], [ActorUserName], [Details])
    VALUES
        (@EntityType, @EntityId, @EntityName, @Action, @ActorUserId, @ActorUserName, @Details);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS AuditLogId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_BulkInsertTestScreenshots]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_BulkRunTestCasesNow]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_BulkRunTestCasesNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        SELECT
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
            @Browser,
            @LoginUserId
        FROM @AssignmentTestCaseIds;

        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

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
/****** Object:  StoredProcedure [aut].[usp_BulkScheduleTestCases]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_BulkScheduleTestCases]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
    @ScheduleDate DATETIME,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        SELECT
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
            @Browser,
            @LoginUserId
        FROM @AssignmentTestCaseIds;

        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

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
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ChangePassword]
    @UserID INT,
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM [aut].[User]
        WHERE UserID = @UserID AND Active = 1
    )
    BEGIN
        SELECT 0;
        RETURN;
    END

    UPDATE [aut].[User]
    SET PasswordHash = @NewPasswordHash
    WHERE UserID = @UserID;

    SELECT 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_CountAutomationDataForSection]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_CountAutomationDataForSection]
    @SectionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM aut.AutomationData WHERE SectionID = @SectionID;
END

GO
/****** Object:  StoredProcedure [aut].[usp_CreateOrUpdateAssignmentWithTestCases]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_CreateOrUpdateAssignmentWithTestCases]
(
    @AssignmentStatus NVARCHAR(100),
    @AssignedUser     INT,
    @ReleaseName      NVARCHAR(255),
    @Environment      NVARCHAR(100),
    @AssignedDate     DATETIME,
    @AssignedBy       INT,
    @LastUpdatedDate  DATETIME,
    @TestCases        aut.TestCaseType READONLY,
    @ReleaseId        INT = NULL,
    @EnvironmentId    INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExistingAssignmentId INT;
    DECLARE @TestCaseCount INT;
    DECLARE @TesterName NVARCHAR(255);
    DECLARE @AssignmentName NVARCHAR(255);
    DECLARE @ResolvedEnvironmentId INT = @EnvironmentId;
    DECLARE @LockedCount INT = 0;

    BEGIN TRANSACTION;

    DECLARE @RealReleaseName NVARCHAR(255) = NULL;

    BEGIN TRY
        SELECT @TesterName = UserName FROM aut.[User] WHERE UserID = @AssignedUser;

        IF @ReleaseId IS NOT NULL
            SELECT @RealReleaseName = ReleaseName FROM aut.Release WHERE ReleaseId = @ReleaseId;

        -- AssignmentName formula is unchanged; when a real Release is linked (@ReleaseId
        -- provided), its actual name is appended as an extra segment so assignments stay
        -- distinct per Release even when Library/Environment match. When @ReleaseId is
        -- NULL, output is byte-for-byte identical to the pre-Release-Management behavior.
        SET @AssignmentName = @TesterName + '-' + @ReleaseName + '-' + @Environment
            + CASE WHEN @RealReleaseName IS NOT NULL THEN '-' + @RealReleaseName ELSE '' END;

        -- Resolve EnvironmentId from text if not supplied
        IF @ResolvedEnvironmentId IS NULL AND @Environment IS NOT NULL
        BEGIN
            SELECT TOP 1 @ResolvedEnvironmentId = EnvironmentId
            FROM aut.[Environment]
            WHERE LTRIM(RTRIM(EnvironmentName)) = LTRIM(RTRIM(@Environment));
        END

        SELECT @ExistingAssignmentId = AssignmentId
        FROM aut.TestCaseAssignment
        WHERE AssignmentName = @AssignmentName
          AND AssignedUser = @AssignedUser;

        SELECT @TestCaseCount = COUNT(*) FROM @TestCases;

        -- Resetting an existing assignment down to zero test cases: only remove test cases
        -- still in the freely-modifiable 'Assigned' state. Locked ones (anything that has
        -- ever entered the execution pipeline) are left in place - and so, since the
        -- assignment then still has test cases, the TestCaseAssignment row itself is left
        -- in place too (not permanently deleted).
        IF @ExistingAssignmentId IS NOT NULL AND @TestCaseCount = 0
        BEGIN
            SELECT @LockedCount = COUNT(*)
            FROM aut.AssignedTestCases
            WHERE AssignmentId = @ExistingAssignmentId
              AND TestCaseStatus <> 'Assigned';

            -- Clean up dependents only for the test cases we're actually about to delete
            -- (the still-'Assigned', unlocked ones).
            DELETE Q
            FROM aut.TestCaseExecutionQueue Q
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = Q.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE S
            FROM aut.TestScreenshots S
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE L
            FROM aut.TestCaseExecutionLogs L
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE FROM aut.AssignedTestCases
            WHERE AssignmentId = @ExistingAssignmentId
              AND TestCaseStatus = 'Assigned';

            IF @LockedCount = 0
            BEGIN
                -- Nothing left locked behind - genuinely empty now, so the assignment
                -- itself is gone too (unchanged from the prior permanent-delete behavior).
                DELETE FROM aut.TestCaseExecutionQueue WHERE AssignmentId = @ExistingAssignmentId;
                DELETE FROM aut.TestCaseAssignment WHERE AssignmentId = @ExistingAssignmentId;
            END
            ELSE
            BEGIN
                UPDATE aut.TestCaseAssignment
                SET LastUpdatedDate = @LastUpdatedDate
                WHERE AssignmentId = @ExistingAssignmentId;
            END

            COMMIT TRANSACTION;
            SELECT @LockedCount AS LockedCount;
            RETURN;
        END

        IF @ExistingAssignmentId IS NULL
        BEGIN
            IF @TestCaseCount = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS LockedCount;
                RETURN;
            END

            INSERT INTO aut.TestCaseAssignment
            (
                AssignmentName, AssignmentStatus, AssignedUser,
                ReleaseName, Environment, ReleaseId, EnvironmentId,
                AssignedDate, AssignedBy, LastUpdatedDate
            )
            VALUES
            (
                @AssignmentName, @AssignmentStatus, @AssignedUser,
                @ReleaseName, @Environment, @ReleaseId, @ResolvedEnvironmentId,
                @AssignedDate, @AssignedBy, @LastUpdatedDate
            );

            SET @ExistingAssignmentId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Keep FK columns current on existing assignments
            UPDATE aut.TestCaseAssignment
            SET ReleaseId       = COALESCE(@ReleaseId, ReleaseId),
                EnvironmentId   = COALESCE(@ResolvedEnvironmentId, EnvironmentId),
                AssignmentStatus = @AssignmentStatus,
                LastUpdatedDate  = @LastUpdatedDate
            WHERE AssignmentId = @ExistingAssignmentId;
        END

        -- Count (for the caller's info) how many locked test cases are being left alone
        -- because the incoming @TestCases omitted them (e.g. a stale client that doesn't
        -- know about a status change) - these are NOT deleted below.
        SELECT @LockedCount = COUNT(*)
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus <> 'Assigned'
          AND NOT EXISTS (SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId);

        -- Clean up dependent rows for test cases about to be removed, BEFORE deleting
        -- AssignedTestCases itself - TestCaseExecutionQueue/TestScreenshots both have FKs
        -- on AssignmentTestCaseId with no cascade, so the delete below would otherwise fail
        -- for any test case that was ever queued/executed or has a screenshot. Only ever
        -- targets still-'Assigned' (unlocked) rows - a locked row is never removed here.
        DELETE Q
        FROM aut.TestCaseExecutionQueue Q
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = Q.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE S
        FROM aut.TestScreenshots S
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE L
        FROM aut.TestCaseExecutionLogs L
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        -- Delete removed test cases (only ones still 'Assigned' - locked ones are kept
        -- even if the caller's @TestCases list omitted them)
        DELETE ATC
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC
                WHERE ATC.TestCaseId = TC.TestCaseId
           );

        -- Insert / update test cases. WHEN MATCHED only fires while the existing row is
        -- still 'Assigned' - once a test case is Queued/Scheduled/InProgress/Passed/
        -- Failed/Cancelled, resending it (e.g. because it's still checked in the UI) no
        -- longer touches it at all.
        MERGE aut.AssignedTestCases AS Target
        USING @TestCases AS Source
        ON Target.AssignmentId = @ExistingAssignmentId
           AND Target.TestCaseId = Source.TestCaseId
        WHEN MATCHED AND Target.TestCaseStatus = 'Assigned' THEN
            UPDATE SET
                TestCaseDescription = Source.TestCaseDescription,
                TestCaseStatus      = Source.TestCaseStatus,
                ClassName           = Source.ClassName,
                LibraryName         = Source.LibraryName,
                MethodName          = Source.MethodName,
                Priority            = Source.Priority
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (AssignmentId, TestCaseId, TestCaseDescription, TestCaseStatus,
                    ClassName, LibraryName, MethodName, Priority)
            VALUES (@ExistingAssignmentId, Source.TestCaseId, Source.TestCaseDescription,
                    Source.TestCaseStatus, Source.ClassName, Source.LibraryName,
                    Source.MethodName, Source.Priority);

        COMMIT TRANSACTION;
        SELECT @LockedCount AS LockedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [aut].[usp_CreateRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*============================================================================
  7. STORED PROCEDURES  (create/alter to match the new schema)
============================================================================*/

-- 7.1 usp_CreateRelease  (adds Version, EnvironmentId, ReleaseFolderPath)
CREATE   PROCEDURE [aut].[usp_CreateRelease]
(
    @ReleaseName       NVARCHAR(100),
    @Version           NVARCHAR(50)  = NULL,
    @EnvironmentId     INT           = NULL,
    @Description       NVARCHAR(255) = NULL,
    @ReleaseFolderPath NVARCHAR(500) = NULL,
    @CreatedBy         NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Uniqueness = ReleaseName + Version + EnvironmentId (business rule)
    IF EXISTS (
        SELECT 1 FROM aut.[Release]
        WHERE ReleaseName = @ReleaseName
          AND ISNULL([Version], N'') = ISNULL(@Version, N'')
          AND ISNULL(EnvironmentId, -1) = ISNULL(@EnvironmentId, -1)
    )
    BEGIN
        RAISERROR('A release with the same Name, Version and Environment already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO aut.[Release]
    (
        ReleaseName, [Version], EnvironmentId, Description, ReleaseFolderPath,
        ReleaseLifecycle, IsActive, SignOffStatus, CreatedBy
    )
    VALUES
    (
        @ReleaseName, @Version, @EnvironmentId, @Description, @ReleaseFolderPath,
        'Draft', 1, 'Pending', @CreatedBy
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_CreateUser]
    @UserName       NVARCHAR(100),
    @PasswordHash    NVARCHAR(500),
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
        PasswordHash,
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
        @PasswordHash,
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
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationData]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSection]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSectionCascade]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_DeleteAutomationDataSectionCascade]
    @SectionID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DELETE FROM aut.AutomationData WHERE SectionID = @SectionID;
        DELETE FROM aut.AutomationDataSections WHERE SectionID = @SectionID;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [aut].[usp_DeleteRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.4c usp_DeleteRelease  (compensating delete; used ONLY to roll back a Release
-- row when the immediately-following physical folder creation fails, so a
-- failed Create never falsely reports success. Cascades to ReleaseDLL,
-- ReleaseNotification, ReleaseSignOff via existing FK ON DELETE CASCADE.)
CREATE   PROCEDURE [aut].[usp_DeleteRelease]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[Release] WHERE ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestScreenshotById]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_EnvironmentCreate]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_EnvironmentCreate]
(
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @CreatedBy INT,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1,
    @EnableSso BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM aut.Environment
        WHERE EnvironmentName = @EnvironmentName
    )
    BEGIN
        RAISERROR ('Environment already exists', 16, 1);
        RETURN;
    END

    -- Authentication Required and Enable SSO are mutually exclusive - normalized here
    -- server-side even though the Portal's own UI already prevents this combination. SSO
    -- wins if somehow both were sent true.
    IF @EnableSso = 1 SET @RequiresAuthentication = 0;

    INSERT INTO aut.Environment
    (
        EnvironmentName,
        Description,
        IsActive,
        CreatedBy,
        EnvironmentUrl,
        RequiresAuthentication,
        EnableSso
    )
    VALUES
    (
        @EnvironmentName,
        @Description,
        1,
        @CreatedBy,
        @EnvironmentUrl,
        @RequiresAuthentication,
        @EnableSso
    );

    SELECT SCOPE_IDENTITY();
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetAll]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_EnvironmentGetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnvironmentId,
        e.EnvironmentName,
        e.Description,
        e.IsActive,
        e.CreatedOn,
        e.EnvironmentUrl,
        e.RequiresAuthentication,
        e.EnableSso,

        u.UserID,
        u.UserName,
        u.Email,

        mu.UserName AS ModifiedByName,

        (SELECT COUNT(*) FROM aut.[Release] r WHERE r.EnvironmentId = e.EnvironmentId) AS ReleaseCount

    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    LEFT JOIN aut.[User] mu ON e.ModifiedBy = mu.UserID
    ORDER BY e.CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_EnvironmentGetById]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.*,
        u.UserName,
        u.Email,
        mu.UserName AS ModifiedByName,
        (SELECT COUNT(*) FROM aut.[Release] r WHERE r.EnvironmentId = e.EnvironmentId) AS ReleaseCount
    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    LEFT JOIN aut.[User] mu ON e.ModifiedBy = mu.UserID
    WHERE e.EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentHardDelete]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. usp_EnvironmentHardDelete - guarded against in-use environments instead of an
-- unconditional DELETE that would otherwise surface a raw FK-violation error.
CREATE   PROCEDURE [aut].[usp_EnvironmentHardDelete]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ReleaseCount INT, @AssignmentCount INT, @AutomationDataCount INT;

    SELECT @ReleaseCount = COUNT(*) FROM aut.[Release] WHERE EnvironmentId = @EnvironmentId;
    SELECT @AssignmentCount = COUNT(*) FROM aut.TestCaseAssignment WHERE EnvironmentId = @EnvironmentId;
    SELECT @AutomationDataCount = COUNT(*) FROM aut.AutomationData WHERE EnvironmentId = @EnvironmentId;

    IF (@ReleaseCount + @AssignmentCount + @AutomationDataCount) > 0
    BEGIN
        RAISERROR('Cannot delete: this environment has %d associated Release(s) and/or other data. Deactivate it instead.', 16, 1, @ReleaseCount);
        RETURN;
    END

    DELETE FROM aut.Environment
    WHERE EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentSoftDelete]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 3. usp_EnvironmentSoftDelete - now also stamps ModifiedBy.
CREATE   PROCEDURE [aut].[usp_EnvironmentSoftDelete]
(
    @EnvironmentId INT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        IsActive = 0,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentUpdate]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_EnvironmentUpdate]
(
    @EnvironmentId INT,
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @IsActive BIT,
    @ModifiedBy INT = NULL,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1,
    @EnableSso BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @EnableSso = 1 SET @RequiresAuthentication = 0;

    UPDATE aut.Environment
    SET
        EnvironmentName = @EnvironmentName,
        Description = @Description,
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME(),
        EnvironmentUrl = @EnvironmentUrl,
        RequiresAuthentication = @RequiresAuthentication,
        EnableSso = @EnableSso
    WHERE EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ForgotPassword]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ForgotPassword]
    @Email NVARCHAR(256),
    @Token NVARCHAR(200),
    @Expiry DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- User does not exist
    IF NOT EXISTS (
        SELECT 1
        FROM [aut].[User]
        WHERE Email = @Email AND Active = 1
    )
    BEGIN
        SELECT 0; -- ❌ User not found
        RETURN;
    END

    UPDATE [aut].[User]
    SET
        ResetPasswordToken = @Token,
        ResetPasswordExpiry = @Expiry
    WHERE Email = @Email
      AND Active = 1;

    SELECT 1; -- ✅ Success
END
GO
/****** Object:  StoredProcedure [aut].[usp_get_AutomationDataSection]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_get_AutomationFlowNames]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAllAssignedTestCasesForRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.13 usp_GetAllAssignedTestCasesForRelease  (NEW - same shape as
--      usp_GetAllAssignedTestCasesInLibrary but spans every library tied to a Release,
--      for the Dashboard's Release-level "Individual Test Case Results" grid)
CREATE   PROCEDURE [aut].[usp_GetAllAssignedTestCasesForRelease]
    @ReleaseId INT
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
        A.Environment,

        CASE
            WHEN EXISTS (
                SELECT 1
                FROM aut.TestScreenshots TS
                WHERE TS.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasScreenshots,

        CASE
            WHEN EXISTS (
                SELECT 1
                FROM aut.TestCaseExecutionLogs L
                WHERE L.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasLogs

    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE
        A.ReleaseId = @ReleaseId
    ORDER BY
        TC.Priority,
        TC.TestCaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAllAssignedTestCasesInLibrary]    Script Date: 04-11-2026 21:35:15 ******/
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
        A.Environment,

        -- NEW: HasScreenshots column
        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM aut.TestScreenshots TS 
                WHERE TS.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasScreenshots,

		 /* 🔥 NEW: HasLogs */
        CASE 
            WHEN EXISTS (
                SELECT 1
                FROM aut.TestCaseExecutionLogs L
                WHERE L.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasLogs
		
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
/****** Object:  StoredProcedure [aut].[usp_GetAllRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.3 usp_GetAllRelease  (join Environment, test summary)
-- NOTE: DLL readiness is filesystem state (DLLs are placed by the existing
-- controlled build/deploy process, not tracked in the database), so it is
-- computed by the application layer (IReleaseReadinessService), not here.
CREATE   PROCEDURE [aut].[usp_GetAllRelease]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.ReleaseId,
        r.ReleaseName,
        r.[Version],
        r.EnvironmentId,
        e.EnvironmentName,
        r.Description,
        r.ReleaseFolderPath,
        r.ReleaseLifecycle,
        r.IsActive,
        r.SignOffStatus,
        r.SignedOffBy,
        r.SignedOffOn,
        r.CreatedBy,
        r.CreatedOn,
        r.ModifiedBy,
        r.ModifiedOn,
        r.ActivatedBy,
        r.ActivatedOn,
        -- Test summary
        ts.TotalTests,
        ts.PassedTests,
        ts.FailedTests,
        ts.SkippedTests,
        ts.RunningTests
    FROM aut.[Release] r
    LEFT JOIN aut.[Environment] e ON r.EnvironmentId = e.EnvironmentId
    OUTER APPLY (
        SELECT
            COUNT(*) AS TotalTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed'  THEN 1 ELSE 0 END) AS PassedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed'  THEN 1 ELSE 0 END) AS FailedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedTests,
            SUM(CASE WHEN atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped') THEN 1 ELSE 0 END) AS RunningTests
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = r.ReleaseId
    ) ts
    ORDER BY r.CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignedTestCasesForLibraryAndRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.11 usp_GetAssignedTestCasesForLibraryAndRelease  (NEW - duplicate-assignment check
--      scoped by Library + Release instead of Library + Environment text, so the same
--      test case can be assigned in two different Releases that share an Environment,
--      but not twice within the same Release)
CREATE   PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndRelease]
(
    @LibraryName NVARCHAR(255),
    @ReleaseId   INT
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
        A.Environment
    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE
        TC.LibraryName = @LibraryName
        AND A.ReleaseId = @ReleaseId
    ORDER BY
        TC.Priority,
        TC.TestCaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAssignmentExecutionLogs]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAssignmentExecutionLogs]
    @AssignmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM aut.TestCaseExecutionLogs
    WHERE AssignmentId = @AssignmentId
    ORDER BY AssignmentTestCaseId, CreatedAt;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetAssignmentReleaseLifecycle]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.14 usp_GetAssignmentReleaseLifecycle  (NEW - lets the execution queue endpoints
--      block Run Now/Schedule once the assignment's linked Release is no longer Active)
CREATE   PROCEDURE [aut].[usp_GetAssignmentReleaseLifecycle]
    @AssignmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT R.ReleaseLifecycle
    FROM aut.TestCaseAssignment TCA
    JOIN aut.Release R ON TCA.ReleaseId = R.ReleaseId
    WHERE TCA.AssignmentId = @AssignmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationData]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*----------------------------------------------------------------------------
  3. Stored procedures
----------------------------------------------------------------------------*/

-- usp_GetAutomationData - now also filters by EnvironmentId
CREATE   PROCEDURE [aut].[usp_GetAutomationData]
    @SectionID INT,
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [ID],
        [SectionID],
        [TestContent],
        [UserID],
        [EnvironmentId]
    FROM [aut].[AutomationData] WITH (NOLOCK)
    WHERE SectionID = @SectionID
      AND UserID = @UserId
      AND EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataByFlowName]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAutomationDataById]
(
    @ID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [ID],
        [SectionID],
        [TestContent],
        [UserID],
        [EnvironmentId]
    FROM [aut].[AutomationData]
    WHERE [ID] = @ID;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataSection]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationFlowNames]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetFilteredUsers]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetPendingExecutionQueues]    Script Date: 04-11-2026 21:35:15 ******/
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
        ATC.AssignmentId,
        ATC.TestCaseId,
        ATC.LibraryName,
        ATC.ClassName,
        ATC.MethodName,
        TCA.Environment,
        TCA.EnvironmentId,
        TCA.ReleaseId,
        TCA.AssignedUser,
        Q.Browser,
        Q.LoginUserId,
        LU.UserName AS LoginUserName,
        Q.QueueStatus,
        Q.ExecutionDateTime

    FROM aut.TestCaseExecutionQueue Q
    INNER JOIN aut.AssignedTestCases ATC
        ON Q.AssignmentTestCaseId = ATC.AssignmentTestCaseId
    INNER JOIN aut.TestCaseAssignment TCA
        ON ATC.AssignmentId = TCA.AssignmentId
    LEFT JOIN aut.LoginUser LU
        ON LU.LoginUserId = Q.LoginUserId

    WHERE
        Q.QueueStatus = 'Queued'
        OR (Q.QueueStatus = 'Scheduled' AND Q.ExecutionDateTime <= GETDATE())

    ORDER BY
        Q.CreatedDate ASC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetPriorityStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetReleaseById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.4 usp_GetReleaseById  (same shape as GetAll, single release)
CREATE   PROCEDURE [aut].[usp_GetReleaseById]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.ReleaseId,
        r.ReleaseName,
        r.[Version],
        r.EnvironmentId,
        e.EnvironmentName,
        r.Description,
        r.ReleaseFolderPath,
        r.ReleaseLifecycle,
        r.IsActive,
        r.SignOffStatus,
        r.SignedOffBy,
        r.SignedOffOn,
        r.CreatedBy,
        r.CreatedOn,
        r.ModifiedBy,
        r.ModifiedOn,
        r.ActivatedBy,
        r.ActivatedOn,
        ts.TotalTests,
        ts.PassedTests,
        ts.FailedTests,
        ts.SkippedTests,
        ts.RunningTests
    FROM aut.[Release] r
    LEFT JOIN aut.[Environment] e ON r.EnvironmentId = e.EnvironmentId
    OUTER APPLY (
        SELECT
            COUNT(*) AS TotalTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed'  THEN 1 ELSE 0 END) AS PassedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed'  THEN 1 ELSE 0 END) AS FailedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedTests,
            SUM(CASE WHEN atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped') THEN 1 ELSE 0 END) AS RunningTests
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = r.ReleaseId
    ) ts
    WHERE r.ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetReleaseExecutionLogs]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.10 usp_GetReleaseExecutionLogs  (ReleaseId-based now that ReleaseId is NOT NULL and
--      reliable, replacing the old @ReleaseName filter which actually matched the
--      historically-misnamed Library-name text column, not the real Release name.
--      Also now selects LogId, previously missing.)
CREATE   PROCEDURE [aut].[usp_GetReleaseExecutionLogs]
    @ReleaseId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.LogId,
        a.ReleaseName,
        a.ReleaseId,
        l.AssignmentId,
        l.AssignmentTestCaseId,
        tc.TestCaseId,
        tc.TestCaseDescription,
        l.StepName,
        l.LogMessage,
        l.LogLevel,
        l.ExecutionStatus,
        l.ErrorStackTrace,
        l.ScreenshotId,
        l.CreatedAt
    FROM aut.TestCaseExecutionLogs l
    JOIN aut.TestCaseAssignment a ON l.AssignmentId = a.AssignmentId
    JOIN aut.AssignedTestCases tc ON l.AssignmentTestCaseId = tc.AssignmentTestCaseId
    WHERE a.ReleaseId = @ReleaseId
    ORDER BY l.CreatedAt;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotById]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByAssignmentTestCaseId]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseAssignmentsByUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.9 usp_GetTestCaseAssignmentsByUser  (additionally return ReleaseId/EnvironmentId)
CREATE   PROCEDURE [aut].[usp_GetTestCaseAssignmentsByUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

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
            A.ReleaseId,
            A.EnvironmentId,
            A.AssignedDate,
            A.AssignedBy,
            UB.UserName AS AssignedByUserName,
            A.LastUpdatedDate
        FROM aut.TestCaseAssignment A
        INNER JOIN aut.[User] U  ON A.AssignedUser = U.UserID
        INNER JOIN aut.[User] UB ON A.AssignedBy   = UB.UserID
        WHERE A.AssignedUser = @UserId;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseExecutionLogs]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetTestCaseExecutionLogs]
    @AssignmentId INT,
    @AssignmentTestCaseId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.ReleaseName,
		l.LogId,
        l.AssignmentId,
        l.AssignmentTestCaseId,

        tc.TestCaseId,
        tc.TestCaseDescription,

        l.StepName,
        l.LogMessage,
        l.LogLevel,
        l.ExecutionStatus,
        l.ScreenshotId,
        l.ErrorStackTrace,
        l.CreatedAt

    FROM aut.TestCaseExecutionLogs l
    INNER JOIN aut.TestCaseAssignment a
        ON l.AssignmentId = a.AssignmentId
    INNER JOIN aut.AssignedTestCases tc
        ON l.AssignmentTestCaseId = tc.AssignmentTestCaseId

    WHERE l.AssignmentId = @AssignmentId
      AND l.AssignmentTestCaseId = @AssignmentTestCaseId

    ORDER BY l.CreatedAt;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestCasesByAssignmentNameAndUser]    Script Date: 04-11-2026 21:35:15 ******/
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
        A.Environment,
        
         -- NEW: HasScreenshots = 1 if screenshots exist
        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM aut.TestScreenshots TS 
                WHERE TS.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasScreenshots,
		 
		 /* 🔥 NEW: HasLogs */
        CASE 
            WHEN EXISTS (
                SELECT 1
                FROM aut.TestCaseExecutionLogs L
                WHERE L.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasLogs

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
/****** Object:  StoredProcedure [aut].[usp_GetTimeZones]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_GetUserById]
    @UserID INT
AS
BEGIN
    SELECT
        usr.UserID,
        usr.UserName,
        usr.PasswordHash,
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
        usr.TwoFactorEnabled AS TwoFactor,
        usr.[TeamsProjects] AS Teams
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
    LEFT JOIN [aut].[PriorityStatus] AS priority ON usr.Priority = priority.PriorityID
    LEFT JOIN [aut].TimeZone AS tz ON usr.TimeZone = tz.TimeZoneID
    LEFT JOIN [aut].[UserStatus] AS us ON us.StatusID = usr.[Status]
    WHERE usr.UserID = @UserID;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetUserByUsername]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_GetUserByUsername]
    @Username NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        usr.UserID,
        usr.UserName,
        usr.PasswordHash,        
        usr.FirstName,
        usr.LastName,
        usr.Email,
		usr.Photo,
        usr.Active,
        role.RoleName,
        role.RoleID
    FROM [aut].[User] usr
    INNER JOIN [aut].[UserRole] role ON usr.RoleID = role.RoleID
    WHERE usr.UserName = @Username
      AND usr.Active = 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUsernameByEmail]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetUsernameByEmail]
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserID,
        UserName,
        Email
    FROM [aut].[User]
    WHERE Email = @Email
      AND Active = 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserRoles]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_GetUserStatuses]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationData]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- usp_InsertAutomationData - now stores EnvironmentId
CREATE   PROCEDURE [aut].[usp_InsertAutomationData]
(
    @SectionID INT = NULL,
    @TestContent NVARCHAR(MAX) = NULL,
    @UserID INT = NULL,
    @EnvironmentId INT = NULL
)
AS
BEGIN TRANSACTION

BEGIN TRY
    INSERT INTO [aut].[AutomationData] (
        [SectionID],
        [TestContent],
        [UserID],
        [EnvironmentId]
    )
    VALUES (
        @SectionID,
        @TestContent,
        @UserID,
        @EnvironmentId
    );

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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationDataSection]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_InsertTestScreenshot]
    @AssignmentTestCaseId INT,
    @Caption NVARCHAR(MAX),
    @Screenshot VARBINARY(MAX),
    @TakenAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.TestScreenshots
        (AssignmentTestCaseId, Caption, Screenshot, TakenAt)
    VALUES
        (@AssignmentTestCaseId, @Caption, @Screenshot, ISNULL(@TakenAt, GETUTCDATE()));

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ScreenshotId;
END;

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserCreate]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 4. Stored procedures

CREATE   PROCEDURE [aut].[usp_LoginUserCreate]
(
    @EnvironmentId INT,
    @PortalUserId INT = NULL,
    @UserRole NVARCHAR(50),
    @UserName NVARCHAR(100),
    @EncryptedPassword NVARCHAR(500),
    @CreatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.LoginUser
    (EnvironmentId, PortalUserId, UserRole, UserName, EncryptedPassword, CreatedBy)
    VALUES
    (@EnvironmentId, @PortalUserId, @UserRole, @UserName, @EncryptedPassword, @CreatedBy);

    SELECT SCOPE_IDENTITY();
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetByEnvironment]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Used by both the Login Users management screen and the Run Now/Schedule dropdowns -
-- never returns EncryptedPassword.
CREATE   PROCEDURE [aut].[usp_LoginUserGetByEnvironment]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lu.LoginUserId,
        lu.EnvironmentId,
        lu.PortalUserId,
        pu.UserName AS PortalUserName,
        lu.UserRole,
        lu.UserName,
        lu.IsActive,
        lu.CreatedOn,
        lu.ModifiedOn
    FROM aut.LoginUser lu
    LEFT JOIN aut.[User] pu ON lu.PortalUserId = pu.UserID
    WHERE lu.EnvironmentId = @EnvironmentId
    ORDER BY lu.CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetByEnvironmentAndPortalUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Same shape as usp_LoginUserGetByEnvironment, plus an owner filter. Used only by the
-- self-service Credential Configuration screen - Run Now/Schedule keep using the
-- existing, unfiltered usp_LoginUserGetByEnvironment (they need to see the credential
-- the person running/scheduling the test picked for themselves via the same ownership
-- filter, resolved by the API from the caller's own JWT - see LoginUserController).
CREATE   PROCEDURE [aut].[usp_LoginUserGetByEnvironmentAndPortalUser]
(
    @EnvironmentId INT,
    @PortalUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lu.LoginUserId,
        lu.EnvironmentId,
        lu.PortalUserId,
        pu.UserName AS PortalUserName,
        lu.UserRole,
        lu.UserName,
        lu.IsActive,
        lu.CreatedOn,
        lu.ModifiedOn
    FROM aut.LoginUser lu
    LEFT JOIN aut.[User] pu ON lu.PortalUserId = pu.UserID
    WHERE lu.EnvironmentId = @EnvironmentId AND lu.PortalUserId = @PortalUserId
    ORDER BY lu.CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_LoginUserGetById]
(
    @LoginUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lu.LoginUserId,
        lu.EnvironmentId,
        lu.PortalUserId,
        pu.UserName AS PortalUserName,
        lu.UserRole,
        lu.UserName,
        lu.IsActive,
        lu.CreatedOn,
        lu.ModifiedOn
    FROM aut.LoginUser lu
    LEFT JOIN aut.[User] pu ON lu.PortalUserId = pu.UserID
    WHERE lu.LoginUserId = @LoginUserId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetCredentials]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- The only procedure that ever returns EncryptedPassword - a plain lookup by primary
-- key, called only by an isolated test process's service JWT (see LoginUserController).
CREATE   PROCEDURE [aut].[usp_LoginUserGetCredentials]
(
    @LoginUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LoginUserId,
        UserName,
        EncryptedPassword
    FROM aut.LoginUser
    WHERE LoginUserId = @LoginUserId AND IsActive = 1;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserHardDelete]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Still guarded like usp_EnvironmentHardDelete (a LoginUserId already used by a real
-- queued/scheduled/executed run can't be hard-deleted without violating the FK) - now
-- also ownership-checked.
CREATE   PROCEDURE [aut].[usp_LoginUserHardDelete]
(
    @LoginUserId INT,
    @PortalUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM aut.LoginUser WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId)
    BEGIN
        SELECT 0 AS RowsAffected;
        RETURN;
    END

    DECLARE @QueueUsageCount INT;

    SELECT @QueueUsageCount = COUNT(*)
    FROM aut.TestCaseExecutionQueue
    WHERE LoginUserId = @LoginUserId;

    IF @QueueUsageCount > 0
    BEGIN
        RAISERROR('Cannot delete: this login user has already been used by %d queued/scheduled run(s). Disable it instead.', 16, 1, @QueueUsageCount);
        RETURN;
    END

    DELETE FROM aut.LoginUser
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserResolveByRole]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Resolves the shared "default" login user for a given Environment+Role, used only by
-- BaseFeatureFixture.LoginByProfile's mid-test role-switch (e.g. TC.Registration logging
-- in as a different role partway through a test) - a genuinely different use case from
-- the initial Run Now/Schedule login (an unattended in-test call, not a human picking
-- from a dropdown), so a role-keyed lookup is appropriate here specifically. Returns the
-- most-recently-created active match for that Environment+Role, or no rows if none
-- configured - caller falls back to a clear failure, never silently to hard-coded data.
CREATE   PROCEDURE [aut].[usp_LoginUserResolveByRole]
(
    @EnvironmentId INT,
    @UserRole NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        LoginUserId,
        UserName,
        EncryptedPassword
    FROM aut.LoginUser
    WHERE EnvironmentId = @EnvironmentId AND UserRole = @UserRole AND IsActive = 1
    ORDER BY CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserSoftDelete]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_LoginUserSoftDelete]
(
    @LoginUserId INT,
    @PortalUserId INT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.LoginUser
    SET
        IsActive = 0,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END

GO
/****** Object:  StoredProcedure [aut].[usp_LoginUserUpdate]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Ownership-enforced: @PortalUserId must match the row's own PortalUserId, or the
-- UPDATE affects 0 rows. A pre-existing row with PortalUserId IS NULL (e.g. old seed/
-- verification data) matches nobody - intentionally not self-service-editable.
CREATE   PROCEDURE [aut].[usp_LoginUserUpdate]
(
    @LoginUserId INT,
    @PortalUserId INT,
    @UserRole NVARCHAR(50),
    @UserName NVARCHAR(100),
    @EncryptedPassword NVARCHAR(500) = NULL, -- NULL = keep existing password unchanged
    @IsActive BIT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.LoginUser
    SET
        UserRole = @UserRole,
        UserName = @UserName,
        EncryptedPassword = COALESCE(@EncryptedPassword, EncryptedPassword),
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END

GO
/****** Object:  StoredProcedure [aut].[usp_Notification_Add]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_Notification_Add]
(
    @UserId           INT,
    @NotificationType NVARCHAR(50),
    @Title             NVARCHAR(200),
    @Message           NVARCHAR(500) = NULL,
    @LinkUrl           NVARCHAR(300) = NULL,
    @SourceType        NVARCHAR(30),
    @SourceId          INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[Notification]
        (UserId, NotificationType, Title, Message, LinkUrl, SourceType, SourceId)
    VALUES
        (@UserId, @NotificationType, @Title, @Message, @LinkUrl, @SourceType, @SourceId);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NotificationId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_Notification_GetByUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Returns all of a user's notifications, newest first - paged client-side by the
-- frontend's shared app-data-grid component, matching every other list page in this
-- app (Users/Environments/Releases/etc. all use pagingMode: 'client'; no page anywhere
-- uses server-side paging today, so introducing a novel OFFSET/FETCH-based SP here
-- would be inconsistent with the rest of the codebase for no real benefit at this
-- data volume).
CREATE   PROCEDURE [aut].[usp_Notification_GetByUser]
(
    @UserId     INT,
    @UnreadOnly BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        NotificationId, UserId, NotificationType, Title, Message, LinkUrl,
        SourceType, SourceId, IsRead, ReadOn, CreatedOn
    FROM aut.[Notification]
    WHERE UserId = @UserId
      AND (@UnreadOnly = 0 OR IsRead = 0)
    ORDER BY CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_Notification_GetUnreadCount]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_Notification_GetUnreadCount]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS UnreadCount
    FROM aut.[Notification]
    WHERE UserId = @UserId AND IsRead = 0;
END

GO
/****** Object:  StoredProcedure [aut].[usp_Notification_MarkAllRead]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_Notification_MarkAllRead]
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[Notification]
    SET IsRead = 1,
        ReadOn = SYSDATETIME()
    WHERE UserId = @UserId AND IsRead = 0;
END

GO
/****** Object:  StoredProcedure [aut].[usp_Notification_MarkRead]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Ownership-checked (WHERE ... AND UserId = @UserId) so one user can never mark
-- another user's notification read via a guessed/enumerated NotificationId.
CREATE   PROCEDURE [aut].[usp_Notification_MarkRead]
(
    @NotificationId INT,
    @UserId         INT
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[Notification]
    SET IsRead = 1,
        ReadOn = SYSDATETIME()
    WHERE NotificationId = @NotificationId AND UserId = @UserId AND IsRead = 0;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_AddRunHistory]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Called by RecurringScheduleWorker right after each firing attempt (Queued/
-- NoEligibleTestCases/Paused). Only increments RunCount for an actual 'Queued' firing -
-- matches the plain-English "how many times has it run".
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_AddRunHistory]
(
    @RecurringScheduleId    INT,
    @Result                 NVARCHAR(30),
    @Detail                 NVARCHAR(300) = NULL,
    @AssignmentTestCaseIds  aut.AssignmentTestCaseIdList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @QueuedCount INT = (SELECT COUNT(*) FROM @AssignmentTestCaseIds);
    DECLARE @ResolutionStatus NVARCHAR(20) = CASE WHEN @Result = 'Queued' AND @QueuedCount > 0 THEN 'Pending' ELSE 'NotApplicable' END;
    DECLARE @RunHistoryId BIGINT;

    INSERT INTO aut.[RecurringScheduleRunHistory]
        (RecurringScheduleId, Result, Detail, TestCasesQueuedCount, ResolutionStatus)
    VALUES
        (@RecurringScheduleId, @Result, @Detail, @QueuedCount, @ResolutionStatus);

    SET @RunHistoryId = SCOPE_IDENTITY();

    IF @QueuedCount > 0
    BEGIN
        INSERT INTO aut.[RecurringScheduleRunHistoryTestCase] (RunHistoryId, AssignmentTestCaseId)
        SELECT @RunHistoryId, AssignmentTestCaseId FROM @AssignmentTestCaseIds;
    END

    IF @Result = 'Queued' AND @QueuedCount > 0
    BEGIN
        UPDATE aut.[RecurringSchedule] SET RunCount = RunCount + 1 WHERE RecurringScheduleId = @RecurringScheduleId;
    END

    SELECT @RunHistoryId AS RunHistoryId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_Create]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RecurringSchedule_Create]
(
    @AssignmentId    INT,
    @RecurrenceType  NVARCHAR(20),
    @DaysOfWeek      NVARCHAR(20) = NULL,
    @DayOfMonth      INT = NULL,
    @TimeOfDay       TIME(0),
    @Browser         NVARCHAR(100),
    @LoginUserId     INT = NULL,
    @EndDate         DATETIME2(7) = NULL,
    @NextRunDate     DATETIME2(7),
    @CreatedBy       NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[RecurringSchedule]
        (AssignmentId, RecurrenceType, DaysOfWeek, DayOfMonth, TimeOfDay, Browser, LoginUserId, EndDate, NextRunDate, CreatedBy)
    VALUES
        (@AssignmentId, @RecurrenceType, @DaysOfWeek, @DayOfMonth, @TimeOfDay, @Browser, @LoginUserId, @EndDate, @NextRunDate, @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_Delete]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RecurringSchedule_Delete]
(
    @RecurringScheduleId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[RecurringSchedule] WHERE RecurringScheduleId = @RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetAll]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Adds RunCount + Login User role/name (mirrors the "${userRole} - ${userName}" display
-- convention already used by the create form's loginUserLabel) to the management list.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, ta.Environment, ta.EnvironmentId,
        r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, lu.UserRole AS LoginUserRole, lu.UserName AS LoginUserName,
        rs.IsActive, rs.PausedReason, rs.EndDate, rs.NextRunDate,
        rs.LastRunDate, rs.RunCount, rs.CreatedBy, rs.CreatedOn
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    LEFT JOIN aut.[LoginUser] lu ON lu.LoginUserId = rs.LoginUserId
    ORDER BY rs.CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetAssignmentOptions]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Lightweight assignment picker list for the Recurring Schedule creation form.
-- EnvironmentId is included so the frontend can resolve Login Users the same
-- self-service way ScheduleTestcasesDialogComponent already does (look up the
-- environment's RequiresAuthentication flag, then the current user's own credential).
-- Only Active-release assignments are offered - matches RecurringScheduleController.
-- Create's own server-side lifecycle check exactly, so the dropdown never lets someone
-- pick something guaranteed to be rejected on submit.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetAssignmentOptions]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ta.AssignmentId, ta.AssignmentName, ta.Environment, ta.EnvironmentId, ta.ReleaseId, r.ReleaseName, r.ReleaseLifecycle
    FROM aut.[TestCaseAssignment] ta
    JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    WHERE r.ReleaseLifecycle = 'Active'
    ORDER BY ta.AssignmentId DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetById]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- EnvironmentId wasn't previously returned by GetAll (only the display-only Environment
-- name string was) - needed here so the Edit form can resolve Login Users for the
-- (read-only, in edit mode) assignment's environment the same self-service way the
-- create form already does.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetById]
(
    @RecurringScheduleId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, ta.Environment, ta.EnvironmentId,
        r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, lu.UserRole AS LoginUserRole, lu.UserName AS LoginUserName,
        rs.IsActive, rs.PausedReason, rs.EndDate, rs.NextRunDate,
        rs.LastRunDate, rs.RunCount, rs.CreatedBy, rs.CreatedOn
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    LEFT JOIN aut.[LoginUser] lu ON lu.LoginUserId = rs.LoginUserId
    WHERE rs.RecurringScheduleId = @RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetDue]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Used by RecurringScheduleWorker each poll cycle - joined to Release.ReleaseLifecycle so
-- the worker can detect a permanently-done (Completed/Rejected) release without a second
-- round-trip.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetDue]
(
    @Now DATETIME2(7)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, rs.EndDate
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    WHERE rs.IsActive = 1
      AND rs.NextRunDate <= @Now
      AND (rs.EndDate IS NULL OR rs.EndDate > @Now);
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetEligibleTestCaseIds]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Current, eligible (not already Queued/Scheduled/InProgress) test cases for an
-- assignment - deliberately does NOT exclude 'Passed' (unlike the one-time Schedule
-- dialog's isTestCaseSelectable), since re-running already-passed tests on a cadence is
-- the entire point of a regression schedule.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetEligibleTestCaseIds]
(
    @AssignmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AssignmentTestCaseId
    FROM aut.[AssignedTestCases]
    WHERE AssignmentId = @AssignmentId
      AND TestCaseStatus NOT IN ('Queued', 'Scheduled', 'InProgress');
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_GetRunHistory]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Used by the new /recurring-schedules/:id/history page.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_GetRunHistory]
(
    @RecurringScheduleId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        RunHistoryId, RecurringScheduleId, RunDate, Result, Detail, TestCasesQueuedCount,
        ResolutionStatus, PassedCount, FailedCount, SkippedCount, ResolvedOn
    FROM aut.[RecurringScheduleRunHistory]
    WHERE RecurringScheduleId = @RecurringScheduleId
    ORDER BY RunDate DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_MarkRun]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RecurringSchedule_MarkRun]
(
    @RecurringScheduleId INT,
    @NextRunDate         DATETIME2(7) = NULL,
    @IsActive            BIT,
    @PausedReason        NVARCHAR(300) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET LastRunDate = SYSDATETIME(),
        NextRunDate = COALESCE(@NextRunDate, NextRunDate),
        IsActive = @IsActive,
        PausedReason = CASE WHEN @IsActive = 1 THEN NULL ELSE @PausedReason END
    WHERE RecurringScheduleId = @RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_ResolvePendingRunHistory]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Set-based sweep, called once per RecurringScheduleWorker poll cycle regardless of
-- whether any schedules were due that cycle. For every 'Pending' history row where none of
-- its snapshotted test cases are still in-flight (Queued/Scheduled/InProgress), computes
-- Passed/Failed/Skipped counts from their *current* status and marks the row Resolved.
CREATE   PROCEDURE [aut].[usp_RecurringSchedule_ResolvePendingRunHistory]
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ReadyToResolve AS (
        SELECT h.RunHistoryId
        FROM aut.[RecurringScheduleRunHistory] h
        WHERE h.ResolutionStatus = 'Pending'
          AND NOT EXISTS (
              SELECT 1
              FROM aut.[RecurringScheduleRunHistoryTestCase] rt
              JOIN aut.[AssignedTestCases] atc ON atc.AssignmentTestCaseId = rt.AssignmentTestCaseId
              WHERE rt.RunHistoryId = h.RunHistoryId
                AND atc.TestCaseStatus IN ('Queued', 'Scheduled', 'InProgress')
          )
    ),
    Outcomes AS (
        SELECT
            rt.RunHistoryId,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed' THEN 1 ELSE 0 END) AS PassedCount,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed' THEN 1 ELSE 0 END) AS FailedCount,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedCount
        FROM aut.[RecurringScheduleRunHistoryTestCase] rt
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentTestCaseId = rt.AssignmentTestCaseId
        WHERE rt.RunHistoryId IN (SELECT RunHistoryId FROM ReadyToResolve)
        GROUP BY rt.RunHistoryId
    )
    UPDATE h
    SET h.ResolutionStatus = 'Resolved',
        h.PassedCount = o.PassedCount,
        h.FailedCount = o.FailedCount,
        h.SkippedCount = o.SkippedCount,
        h.ResolvedOn = SYSDATETIME()
    FROM aut.[RecurringScheduleRunHistory] h
    JOIN Outcomes o ON o.RunHistoryId = h.RunHistoryId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_SetActive]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RecurringSchedule_SetActive]
(
    @RecurringScheduleId INT,
    @IsActive            BIT,
    @PausedReason        NVARCHAR(300) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET IsActive = @IsActive,
        PausedReason = CASE WHEN @IsActive = 1 THEN NULL ELSE @PausedReason END
    WHERE RecurringScheduleId = @RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RecurringSchedule_Update]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RecurringSchedule_Update]
(
    @RecurringScheduleId INT,
    @RecurrenceType      NVARCHAR(20),
    @DaysOfWeek          NVARCHAR(20) = NULL,
    @DayOfMonth          INT = NULL,
    @TimeOfDay           TIME(0),
    @Browser             NVARCHAR(100),
    @LoginUserId         INT = NULL,
    @EndDate             DATETIME2(7) = NULL,
    @NextRunDate         DATETIME2(7)
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET RecurrenceType = @RecurrenceType,
        DaysOfWeek = @DaysOfWeek,
        DayOfMonth = @DayOfMonth,
        TimeOfDay = @TimeOfDay,
        Browser = @Browser,
        LoginUserId = @LoginUserId,
        EndDate = @EndDate,
        NextRunDate = @NextRunDate
    WHERE RecurringScheduleId = @RecurringScheduleId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_RegisterUser]
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500)
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

    -- Default Status = Active (matches the Active bit below - not an approval gate)
    SELECT @StatusID = StatusID
    FROM [aut].[UserStatus]
    WHERE StatusName = 'Active';

    IF (@StatusID IS NULL)
    BEGIN
        THROW 50002, 'Default status ''Active'' not found in UserStatus table.', 1;
        RETURN;
    END;

    -- Insert User
    INSERT INTO [aut].[User]
    (
        UserName,
        Email,
        PasswordHash,
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
        @PasswordHash,
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
/****** Object:  StoredProcedure [aut].[usp_Release_SetFolderPath]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.4b usp_Release_SetFolderPath  (store the resolved folder path after physical creation)
CREATE   PROCEDURE [aut].[usp_Release_SetFolderPath]
(
    @ReleaseId INT,
    @ReleaseFolderPath NVARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[Release]
    SET ReleaseFolderPath = @ReleaseFolderPath,
        ModifiedOn = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_Add]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.7 ReleaseNotification stored procedures
CREATE   PROCEDURE [aut].[usp_ReleaseNotification_Add]
(
    @ReleaseId        INT,
    @NotificationType NVARCHAR(50),
    @RecipientUserId  INT = NULL,
    @RecipientEmail   NVARCHAR(255) = NULL,
    @Message          NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[ReleaseNotification]
        (ReleaseId, NotificationType, RecipientUserId, RecipientEmail, Status, Message)
    VALUES
        (@ReleaseId, @NotificationType, @RecipientUserId, @RecipientEmail, 'Pending', @Message);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReleaseNotificationId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_GetByRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_ReleaseNotification_GetByRelease]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReleaseNotificationId, ReleaseId, NotificationType, RecipientUserId,
           RecipientEmail, Status, Message, CreatedOn, SentOn
    FROM aut.[ReleaseNotification]
    WHERE ReleaseId = @ReleaseId
    ORDER BY CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_MarkSent]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_ReleaseNotification_MarkSent]
(
    @ReleaseNotificationId INT,
    @Status                NVARCHAR(30) = 'Sent'
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[ReleaseNotification]
    SET Status = @Status,
        SentOn = CASE WHEN @Status = 'Sent' THEN SYSDATETIME() ELSE SentOn END
    WHERE ReleaseNotificationId = @ReleaseNotificationId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.6 usp_ReleaseSignOff  (only after all assigned tests are terminal; approve/reject + history)
CREATE   PROCEDURE [aut].[usp_ReleaseSignOff]
(
    @ReleaseId     INT,
    @SignOffStatus NVARCHAR(20),      -- 'Approved' or 'Rejected'
    @SignedOffBy   NVARCHAR(100),
    @Comments      NVARCHAR(1000) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @SignOffStatus NOT IN ('Approved', 'Rejected')
    BEGIN
        RAISERROR('SignOffStatus must be Approved or Rejected.', 16, 1); RETURN;
    END

    -- Testing must be complete: no non-terminal assigned test cases for this release
    IF EXISTS (
        SELECT 1
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = @ReleaseId
          AND atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped')
    )
    BEGIN
        RAISERROR('Cannot sign off: not all assigned tests have completed.', 16, 1); RETURN;
    END

    -- A release with zero assigned tests cannot be signed off either
    IF NOT EXISTS (
        SELECT 1
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = @ReleaseId
    )
    BEGIN
        RAISERROR('Cannot sign off: the release has no completed tests to review.', 16, 1); RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO aut.[ReleaseSignOff] (ReleaseId, SignOffStatus, SignOffBy, SignOffOn, Comments)
        VALUES (@ReleaseId, @SignOffStatus, @SignedOffBy, SYSDATETIME(), @Comments);

        UPDATE aut.[Release]
        SET SignOffStatus    = @SignOffStatus,
            SignedOffBy      = @SignedOffBy,
            SignedOffOn      = SYSDATETIME(),
            ReleaseLifecycle = CASE WHEN @SignOffStatus = 'Approved' THEN 'Completed' ELSE 'Rejected' END,
            ModifiedBy       = @SignedOffBy,
            ModifiedOn       = SYSDATETIME()
        WHERE ReleaseId = @ReleaseId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff_GetByRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.6b usp_ReleaseSignOff_GetByRelease  (sign-off history for a release)
CREATE   PROCEDURE [aut].[usp_ReleaseSignOff_GetByRelease]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReleaseSignOffId, ReleaseId, SignOffStatus, SignOffBy, SignOffOn, Comments, CreatedOn
    FROM aut.[ReleaseSignOff]
    WHERE ReleaseId = @ReleaseId
    ORDER BY CreatedOn DESC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ResetPassword]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ResetPassword]
    @Token NVARCHAR(200),
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate token
    IF NOT EXISTS (
        SELECT 1
        FROM [aut].[User]
        WHERE ResetPasswordToken = @Token
          AND ResetPasswordExpiry > GETUTCDATE()
          AND Active = 1
    )
    BEGIN
        SELECT 0; -- ❌ Invalid or expired token
        RETURN;
    END

    -- Update password and clear token
    UPDATE [aut].[User]
    SET
        PasswordHash = @NewPasswordHash,
        ResetPasswordToken = NULL,
        ResetPasswordExpiry = NULL
    WHERE ResetPasswordToken = @Token
      AND Active = 1;

    SELECT 1; -- ✅ Success
END
GO
/****** Object:  StoredProcedure [aut].[usp_ScheduleSingleTestCase]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_ScheduleSingleTestCase]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @ScheduleDate DATETIME,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
            @Browser,
            @LoginUserId
        );

        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        SELECT
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;

GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_SingleRunTestCaseNow]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. Queue insert procs + pending-queue getter: thread LoginUserId (optional) through.

CREATE   PROCEDURE [aut].[usp_SingleRunTestCaseNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
            @Browser,
            @LoginUserId
        );

        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        SELECT
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestCaseDataGet]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestCaseDataGet]
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TD.TestCaseDataId, TD.FlowName, TD.DataJson,
           CAST(TD.[Version] AS BIGINT) AS [Version],
           COALESCE(TD.ModifiedOn, TD.CreatedOn) AS ModifiedOn
    FROM aut.TestCaseData TD
    JOIN aut.TestCaseKey K ON K.TestCaseKeyId = TD.TestCaseKeyId
    WHERE K.LibraryName = @LibraryName AND K.ClassName = @ClassName AND K.MethodName = @MethodName
      AND TD.UserID = @UserId AND TD.EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestCaseDataGetCopySources]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestCaseDataGetCopySources]
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @FlowName NVARCHAR(250),
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT K.LibraryName, K.ClassName, K.MethodName,
        (SELECT TOP 1 ATC.TestCaseId FROM aut.AssignedTestCases ATC
         WHERE ATC.LibraryName = K.LibraryName AND ATC.ClassName = K.ClassName AND ATC.MethodName = K.MethodName
         ORDER BY ATC.AssignmentTestCaseId DESC) AS TestCaseId
    FROM aut.TestCaseData TD
    JOIN aut.TestCaseKey K ON K.TestCaseKeyId = TD.TestCaseKeyId
    WHERE TD.FlowName = @FlowName AND TD.UserID = @UserId AND TD.EnvironmentId = @EnvironmentId
      AND NOT (K.LibraryName = @LibraryName AND K.ClassName = @ClassName AND K.MethodName = @MethodName);
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestCaseDataSave]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestCaseDataSave]
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @UserId INT,
    @EnvironmentId INT,
    @FlowName NVARCHAR(250),
    @DataJson NVARCHAR(MAX),
    @ExpectedVersion BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @KeyId INT;
    SELECT @KeyId = TestCaseKeyId FROM aut.TestCaseKey WITH (UPDLOCK, HOLDLOCK)
    WHERE LibraryName = @LibraryName AND ClassName = @ClassName AND MethodName = @MethodName;

    IF @KeyId IS NULL
    BEGIN
        INSERT INTO aut.TestCaseKey (LibraryName, ClassName, MethodName) VALUES (@LibraryName, @ClassName, @MethodName);
        SET @KeyId = SCOPE_IDENTITY();
    END

    DECLARE @Id INT, @CurrentVersion BIGINT;
    SELECT @Id = TestCaseDataId, @CurrentVersion = CAST([Version] AS BIGINT)
    FROM aut.TestCaseData WITH (UPDLOCK, HOLDLOCK)
    WHERE TestCaseKeyId = @KeyId AND UserID = @UserId AND EnvironmentId = @EnvironmentId;

    IF @Id IS NULL
    BEGIN
        IF @ExpectedVersion IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(NULL AS INT) AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(1 AS BIT) AS IsConflict, CAST(NULL AS BIGINT) AS [Version];
            RETURN;
        END

        INSERT INTO aut.TestCaseData (TestCaseKeyId, UserID, EnvironmentId, FlowName, DataJson)
        VALUES (@KeyId, @UserId, @EnvironmentId, @FlowName, @DataJson);
        SET @Id = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
        SELECT @Id AS TestCaseDataId, CAST(1 AS BIT) AS IsNew, CAST(0 AS BIT) AS IsConflict,
               (SELECT CAST([Version] AS BIGINT) FROM aut.TestCaseData WHERE TestCaseDataId = @Id) AS [Version];
        RETURN;
    END

    IF @ExpectedVersion IS NULL OR @ExpectedVersion <> @CurrentVersion
    BEGIN
        COMMIT TRANSACTION;
        SELECT @Id AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(1 AS BIT) AS IsConflict, @CurrentVersion AS [Version];
        RETURN;
    END

    UPDATE aut.TestCaseData
    SET DataJson = @DataJson, FlowName = @FlowName, ModifiedOn = SYSDATETIME()
    WHERE TestCaseDataId = @Id;

    COMMIT TRANSACTION;
    SELECT @Id AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(0 AS BIT) AS IsConflict,
           (SELECT CAST([Version] AS BIGINT) FROM aut.TestCaseData WHERE TestCaseDataId = @Id) AS [Version];
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestDataGetMyTestCases]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestDataGetMyTestCases]
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Ranked AS (
        SELECT
            ATC.AssignmentTestCaseId, ATC.LibraryName, ATC.ClassName, ATC.MethodName,
            ATC.TestCaseId, ATC.TestCaseDescription, TCA.ReleaseId,
            ROW_NUMBER() OVER (PARTITION BY ATC.LibraryName, ATC.ClassName, ATC.MethodName
                               ORDER BY ATC.AssignmentTestCaseId DESC) AS rn
        FROM aut.AssignedTestCases ATC
        JOIN aut.TestCaseAssignment TCA ON TCA.AssignmentId = ATC.AssignmentId
        WHERE TCA.AssignedUser = @UserId
          AND TCA.EnvironmentId = @EnvironmentId
          AND ATC.LibraryName IS NOT NULL AND ATC.ClassName IS NOT NULL AND ATC.MethodName IS NOT NULL
    )
    SELECT AssignmentTestCaseId, LibraryName, ClassName, MethodName, TestCaseId, TestCaseDescription, ReleaseId
    FROM Ranked
    WHERE rn = 1
    ORDER BY TestCaseId, ClassName, MethodName;
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestDataResolveAssignment]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestDataResolveAssignment]
    @AssignmentTestCaseId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ATC.AssignmentTestCaseId, ATC.LibraryName, ATC.ClassName, ATC.MethodName,
        ATC.TestCaseId, TCA.AssignedUser, TCA.EnvironmentId, TCA.ReleaseId, R.ReleaseFolderPath
    FROM aut.AssignedTestCases ATC
    JOIN aut.TestCaseAssignment TCA ON TCA.AssignmentId = ATC.AssignmentId
    LEFT JOIN aut.[Release] R ON R.ReleaseId = TCA.ReleaseId
    WHERE ATC.AssignmentTestCaseId = @AssignmentTestCaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestExecutionNotification_Add]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestExecutionNotification_Add]
(
    @AssignmentTestCaseId INT,
    @NotificationType     NVARCHAR(50),
    @RecipientUserId      INT = NULL,
    @RecipientEmail       NVARCHAR(255) = NULL,
    @Message              NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[TestExecutionNotification]
        (AssignmentTestCaseId, NotificationType, RecipientUserId, RecipientEmail, Status, Message)
    VALUES
        (@AssignmentTestCaseId, @NotificationType, @RecipientUserId, @RecipientEmail, 'Pending', @Message);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS TestExecutionNotificationId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_TestExecutionNotification_MarkSent]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_TestExecutionNotification_MarkSent]
(
    @TestExecutionNotificationId INT,
    @Status                      NVARCHAR(30) = 'Sent'
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[TestExecutionNotification]
    SET Status = @Status,
        SentOn = CASE WHEN @Status = 'Sent' THEN SYSDATETIME() ELSE SentOn END
    WHERE TestExecutionNotificationId = @TestExecutionNotificationId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_UpdateAssignedTestCaseStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationData]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationDataSections]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateLastLogin]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_UpdateLastLogin]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[User]
    SET LastLogin = GETDATE()
    WHERE UserID = @UserID
      AND Active = 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateQueueStatus]    Script Date: 04-11-2026 21:35:15 ******/
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
/****** Object:  StoredProcedure [aut].[usp_UpdateRelease]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 7.2 usp_UpdateRelease  (adds Version/EnvironmentId/ReleaseFolderPath/ModifiedBy)
CREATE   PROCEDURE [aut].[usp_UpdateRelease]
(
    @ReleaseId         INT,
    @ReleaseName       NVARCHAR(100),
    @Version           NVARCHAR(50)  = NULL,
    @EnvironmentId     INT           = NULL,
    @Description       NVARCHAR(255) = NULL,
    @ReleaseFolderPath NVARCHAR(500) = NULL,
    @ReleaseLifecycle  NVARCHAR(30)  = NULL,
    @IsActive          BIT           = NULL,
    @ModifiedBy        NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.[Release]
    SET
        ReleaseName       = @ReleaseName,
        [Version]         = @Version,
        EnvironmentId     = @EnvironmentId,
        Description       = @Description,
        ReleaseFolderPath = COALESCE(@ReleaseFolderPath, ReleaseFolderPath),
        ReleaseLifecycle  = COALESCE(@ReleaseLifecycle, ReleaseLifecycle),
        IsActive          = COALESCE(@IsActive, IsActive),
        ModifiedBy        = @ModifiedBy,
        ModifiedOn        = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateUser]
    @UserID INT,
    @UserName       NVARCHAR(100),    
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
/****** Object:  StoredProcedure [aut].[usp_UpdateUserProfile]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_UpdateUserProfile]
    @UserId      INT,
    @Photo       VARBINARY(MAX) = NULL,
    @PhoneNumber NVARCHAR(20)   = NULL,
    @TimeZone    INT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[User]
    SET
        Photo       = COALESCE(@Photo, Photo),
        PhoneNumber = @PhoneNumber,
        TimeZone    = @TimeZone
    WHERE UserID = @UserId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_UpsertModelSections]    Script Date: 04-11-2026 21:35:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_UpsertModelSections]
    @FlowName NVARCHAR(250),
    @SectionNames NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Names TABLE (SectionName NVARCHAR(250) PRIMARY KEY);
    INSERT INTO @Names (SectionName)
    SELECT DISTINCT value FROM OPENJSON(@SectionNames);

    IF EXISTS (
        SELECT 1 FROM @Names N
        WHERE NOT EXISTS (
            SELECT 1 FROM aut.AutomationDataSections S
            WHERE S.FlowName = @FlowName AND S.SectionName = N.SectionName AND S.IsFromModel = 1)
    )
    BEGIN
        BEGIN TRANSACTION;

        DECLARE @Resource NVARCHAR(255) = N'aut.ModelSections:' + LEFT(@FlowName, 200);
        EXEC sp_getapplock @Resource = @Resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;

        UPDATE S SET IsFromModel = 1
        FROM aut.AutomationDataSections S
        JOIN @Names N ON N.SectionName = S.SectionName
        WHERE S.FlowName = @FlowName AND S.IsFromModel = 0;

        INSERT INTO aut.AutomationDataSections (SectionName, FlowName, IsFromModel)
        SELECT N.SectionName, @FlowName, 1
        FROM @Names N
        WHERE NOT EXISTS (
            SELECT 1 FROM aut.AutomationDataSections S
            WHERE S.FlowName = @FlowName AND S.SectionName = N.SectionName
        );

        COMMIT TRANSACTION;
    END

    SELECT SectionID, SectionName, FlowName, IsFromModel
    FROM aut.AutomationDataSections
    WHERE FlowName = @FlowName;
END

