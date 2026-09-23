/****** Object:  Schema [aut]    Script Date: 23-09-2026 10:30:12 ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'aut')
EXEC sys.sp_executesql N'CREATE SCHEMA [aut]'
GO
/****** Object:  UserDefinedTableType [aut].[AssignmentTestCaseIdList]    Script Date: 23-09-2026 10:30:12 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'AssignmentTestCaseIdList' AND ss.name = N'aut')
CREATE TYPE [aut].[AssignmentTestCaseIdList] AS TABLE(
	[AssignmentTestCaseId] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [aut].[TestCaseType]    Script Date: 23-09-2026 10:30:12 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TestCaseType' AND ss.name = N'aut')
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
/****** Object:  UserDefinedTableType [aut].[TestScreenshotType]    Script Date: 23-09-2026 10:30:12 ******/
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'TestScreenshotType' AND ss.name = N'aut')
CREATE TYPE [aut].[TestScreenshotType] AS TABLE(
	[AssignmentTestCaseId] [int] NULL,
	[Caption] [nvarchar](max) NULL,
	[Screenshot] [varbinary](max) NULL,
	[TakenAt] [datetime] NULL
)
GO
/****** Object:  Table [aut].[AssignedTestCases]    Script Date: 23-09-2026 10:30:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[AssignedTestCases]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[AutomationData]    Script Date: 23-09-2026 10:30:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[AutomationData]') AND type in (N'U'))
BEGIN
CREATE TABLE [aut].[AutomationData](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SectionID] [int] NULL,
	[TestContent] [varchar](max) NULL,
	[UserID] [int] NULL,
	[EnvironmentId] [int] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [aut].[AutomationDataSections]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[AutomationDataSections]') AND type in (N'U'))
BEGIN
CREATE TABLE [aut].[AutomationDataSections](
	[SectionID] [int] IDENTITY(1,1) NOT NULL,
	[SectionName] [nvarchar](250) NULL,
	[FlowName] [nvarchar](250) NULL
) ON [PRIMARY]
END
GO
/****** Object:  Table [aut].[Environment]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Environment]') AND type in (N'U'))
BEGIN
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
PRIMARY KEY CLUSTERED 
(
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [aut].[LoginUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[LoginUser]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[PriorityStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[PriorityStatus]') AND type in (N'U'))
BEGIN
CREATE TABLE [aut].[PriorityStatus](
	[PriorityID] [int] NOT NULL,
	[PriorityName] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PriorityID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [aut].[Release]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Release]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[ReleaseNotification]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[ReleaseNotification]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[ReleaseSignOff]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[ReleaseSignOff]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TestCaseAssignment]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestCaseAssignment]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TestCaseExecutionLogs]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TestCaseExecutionQueue]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TestExecutionNotification]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestExecutionNotification]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TestScreenshots]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TestScreenshots]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[TimeZone]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[TimeZone]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[User]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[User]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [aut].[UserRole]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[UserRole]') AND type in (N'U'))
BEGIN
CREATE TABLE [aut].[UserRole](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [aut].[UserStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[UserStatus]') AND type in (N'U'))
BEGIN
CREATE TABLE [aut].[UserStatus](
	[StatusID] [int] NOT NULL,
	[StatusName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[StatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Environme__IsAct__534D60F1]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((1)) FOR [IsActive]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Environme__Creat__5441852A]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Environment] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Environme__Requi__489AC854]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((1)) FOR [RequiresAuthentication]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__LoginUser__IsAct__4B7734FF]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[LoginUser] ADD  DEFAULT ((1)) FOR [IsActive]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__LoginUser__Creat__4C6B5938]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[LoginUser] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Release__Release__5535A963]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Draft') FOR [ReleaseLifecycle]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Release__IsActiv__5629CD9C]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Release] ADD  DEFAULT ((1)) FOR [IsActive]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Release__SignOff__571DF1D5]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Pending') FOR [SignOffStatus]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__Release__Created__5812160E]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[Release] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_ReleaseNotification_Status]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[ReleaseNotification] ADD  CONSTRAINT [DF_ReleaseNotification_Status]  DEFAULT ('Pending') FOR [Status]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_ReleaseNotification_CreatedOn]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[ReleaseNotification] ADD  CONSTRAINT [DF_ReleaseNotification_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_ReleaseSignOff_CreatedOn]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[ReleaseSignOff] ADD  CONSTRAINT [DF_ReleaseSignOff_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__TestCaseE__Creat__59063A47]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[TestCaseExecutionLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__TestCaseE__Queue__59FA5E80]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[TestCaseExecutionQueue] ADD  DEFAULT (newid()) FOR [QueueId]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_TestExecutionNotification_Status]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[TestExecutionNotification] ADD  CONSTRAINT [DF_TestExecutionNotification_Status]  DEFAULT ('Pending') FOR [Status]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_TestExecutionNotification_CreatedOn]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[TestExecutionNotification] ADD  CONSTRAINT [DF_TestExecutionNotification_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF__TestScree__Taken__5AEE82B9]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[TestScreenshots] ADD  DEFAULT (getutcdate()) FOR [TakenAt]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_User_Active]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[User] ADD  CONSTRAINT [DF_User_Active]  DEFAULT ((1)) FOR [Active]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[DF_User_TwoFactorEnabled]') AND type = 'D')
BEGIN
ALTER TABLE [aut].[User] ADD  CONSTRAINT [DF_User_TwoFactorEnabled]  DEFAULT ((0)) FOR [TwoFactorEnabled]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_AssignedTestCases_Assignment]') AND parent_object_id = OBJECT_ID(N'[aut].[AssignedTestCases]'))
ALTER TABLE [aut].[AssignedTestCases]  WITH CHECK ADD  CONSTRAINT [FK_AssignedTestCases_Assignment] FOREIGN KEY([AssignmentId])
REFERENCES [aut].[TestCaseAssignment] ([AssignmentId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_AssignedTestCases_Assignment]') AND parent_object_id = OBJECT_ID(N'[aut].[AssignedTestCases]'))
ALTER TABLE [aut].[AssignedTestCases] CHECK CONSTRAINT [FK_AssignedTestCases_Assignment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_AutomationData_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[AutomationData]'))
ALTER TABLE [aut].[AutomationData]  WITH CHECK ADD  CONSTRAINT [FK_AutomationData_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_AutomationData_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[AutomationData]'))
ALTER TABLE [aut].[AutomationData] CHECK CONSTRAINT [FK_AutomationData_Environment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Environment_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[Environment]'))
ALTER TABLE [aut].[Environment]  WITH CHECK ADD  CONSTRAINT [FK_Environment_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Environment_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[Environment]'))
ALTER TABLE [aut].[Environment] CHECK CONSTRAINT [FK_Environment_CreatedBy]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Environment_ModifiedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[Environment]'))
ALTER TABLE [aut].[Environment]  WITH CHECK ADD  CONSTRAINT [FK_Environment_ModifiedBy] FOREIGN KEY([ModifiedBy])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Environment_ModifiedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[Environment]'))
ALTER TABLE [aut].[Environment] CHECK CONSTRAINT [FK_Environment_ModifiedBy]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_CreatedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_CreatedBy]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_Environment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_ModifiedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_ModifiedBy] FOREIGN KEY([ModifiedBy])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_ModifiedBy]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_ModifiedBy]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_PortalUser]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser]  WITH CHECK ADD  CONSTRAINT [FK_LoginUser_PortalUser] FOREIGN KEY([PortalUserId])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_LoginUser_PortalUser]') AND parent_object_id = OBJECT_ID(N'[aut].[LoginUser]'))
ALTER TABLE [aut].[LoginUser] CHECK CONSTRAINT [FK_LoginUser_PortalUser]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Release_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[Release]'))
ALTER TABLE [aut].[Release]  WITH CHECK ADD  CONSTRAINT [FK_Release_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_Release_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[Release]'))
ALTER TABLE [aut].[Release] CHECK CONSTRAINT [FK_Release_Environment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseNotification_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseNotification]'))
ALTER TABLE [aut].[ReleaseNotification]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseNotification_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseNotification_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseNotification]'))
ALTER TABLE [aut].[ReleaseNotification] CHECK CONSTRAINT [FK_ReleaseNotification_Release]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseNotification_User]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseNotification]'))
ALTER TABLE [aut].[ReleaseNotification]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseNotification_User] FOREIGN KEY([RecipientUserId])
REFERENCES [aut].[User] ([UserID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseNotification_User]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseNotification]'))
ALTER TABLE [aut].[ReleaseNotification] CHECK CONSTRAINT [FK_ReleaseNotification_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseSignOff_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseSignOff]'))
ALTER TABLE [aut].[ReleaseSignOff]  WITH CHECK ADD  CONSTRAINT [FK_ReleaseSignOff_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_ReleaseSignOff_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[ReleaseSignOff]'))
ALTER TABLE [aut].[ReleaseSignOff] CHECK CONSTRAINT [FK_ReleaseSignOff_Release]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseAssignment_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseAssignment]'))
ALTER TABLE [aut].[TestCaseAssignment]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseAssignment_Environment] FOREIGN KEY([EnvironmentId])
REFERENCES [aut].[Environment] ([EnvironmentId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseAssignment_Environment]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseAssignment]'))
ALTER TABLE [aut].[TestCaseAssignment] CHECK CONSTRAINT [FK_TestCaseAssignment_Environment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseAssignment_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseAssignment]'))
ALTER TABLE [aut].[TestCaseAssignment]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseAssignment_Release] FOREIGN KEY([ReleaseId])
REFERENCES [aut].[Release] ([ReleaseId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseAssignment_Release]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseAssignment]'))
ALTER TABLE [aut].[TestCaseAssignment] CHECK CONSTRAINT [FK_TestCaseAssignment_Release]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_AssignedTestCase]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_AssignedTestCase] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_AssignedTestCase]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_AssignedTestCase]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_Assignment]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_Assignment] FOREIGN KEY([AssignmentId])
REFERENCES [aut].[TestCaseAssignment] ([AssignmentId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_Assignment]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_Assignment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_LoginUser]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseExecutionQueue_LoginUser] FOREIGN KEY([LoginUserId])
REFERENCES [aut].[LoginUser] ([LoginUserId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestCaseExecutionQueue_LoginUser]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionQueue]'))
ALTER TABLE [aut].[TestCaseExecutionQueue] CHECK CONSTRAINT [FK_TestCaseExecutionQueue_LoginUser]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestExecutionNotification_AssignedTestCases]') AND parent_object_id = OBJECT_ID(N'[aut].[TestExecutionNotification]'))
ALTER TABLE [aut].[TestExecutionNotification]  WITH CHECK ADD  CONSTRAINT [FK_TestExecutionNotification_AssignedTestCases] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestExecutionNotification_AssignedTestCases]') AND parent_object_id = OBJECT_ID(N'[aut].[TestExecutionNotification]'))
ALTER TABLE [aut].[TestExecutionNotification] CHECK CONSTRAINT [FK_TestExecutionNotification_AssignedTestCases]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestScreenshots_AssignedTestCases]') AND parent_object_id = OBJECT_ID(N'[aut].[TestScreenshots]'))
ALTER TABLE [aut].[TestScreenshots]  WITH CHECK ADD  CONSTRAINT [FK_TestScreenshots_AssignedTestCases] FOREIGN KEY([AssignmentTestCaseId])
REFERENCES [aut].[AssignedTestCases] ([AssignmentTestCaseId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_TestScreenshots_AssignedTestCases]') AND parent_object_id = OBJECT_ID(N'[aut].[TestScreenshots]'))
ALTER TABLE [aut].[TestScreenshots] CHECK CONSTRAINT [FK_TestScreenshots_AssignedTestCases]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK__User__RoleID__628FA481]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User]  WITH CHECK ADD FOREIGN KEY([RoleID])
REFERENCES [aut].[UserRole] ([RoleID])
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_Priority]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Priority] FOREIGN KEY([Priority])
REFERENCES [aut].[PriorityStatus] ([PriorityID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_Priority]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_Priority]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_Status]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_Status] FOREIGN KEY([Status])
REFERENCES [aut].[UserStatus] ([StatusID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_Status]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_Status]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_TimeZone]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User]  WITH CHECK ADD  CONSTRAINT [FK_User_TimeZone] FOREIGN KEY([TimeZone])
REFERENCES [aut].[TimeZone] ([TimeZoneID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[aut].[FK_User_TimeZone]') AND parent_object_id = OBJECT_ID(N'[aut].[User]'))
ALTER TABLE [aut].[User] CHECK CONSTRAINT [FK_User_TimeZone]
GO
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_Consistency]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_Consistency] CHECK  (([LogLevel]='Fail' AND [ExecutionStatus]='Failed' OR [LogLevel]='Warning' AND [ExecutionStatus]='Running' OR [LogLevel]='Info' AND [ExecutionStatus]='Running' OR [LogLevel]='Pass' AND ([ExecutionStatus]='Passed' OR [ExecutionStatus]='Running')))
GO
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_Consistency]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_Consistency]
GO
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_ExecutionStatus]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_ExecutionStatus] CHECK  (([ExecutionStatus]='Failed' OR [ExecutionStatus]='Passed' OR [ExecutionStatus]='Running'))
GO
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_ExecutionStatus]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_ExecutionStatus]
GO
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_LogLevel]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs]  WITH CHECK ADD  CONSTRAINT [CK_TestCaseExecutionLogs_LogLevel] CHECK  (([LogLevel]='Fail' OR [LogLevel]='Warning' OR [LogLevel]='Pass' OR [LogLevel]='Info'))
GO
IF  EXISTS (SELECT * FROM sys.check_constraints WHERE object_id = OBJECT_ID(N'[aut].[CK_TestCaseExecutionLogs_LogLevel]') AND parent_object_id = OBJECT_ID(N'[aut].[TestCaseExecutionLogs]'))
ALTER TABLE [aut].[TestCaseExecutionLogs] CHECK CONSTRAINT [CK_TestCaseExecutionLogs_LogLevel]
GO
/****** Object:  StoredProcedure [aut].[usp_ActivateRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ActivateRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ActivateRelease] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_ActivateRelease]
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
/****** Object:  StoredProcedure [aut].[usp_AddTestCaseExecutionLog]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_AddTestCaseExecutionLog]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_AddTestCaseExecutionLog] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_AddTestCaseExecutionLog]
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
/****** Object:  StoredProcedure [aut].[usp_BulkInsertTestScreenshots]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_BulkInsertTestScreenshots]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_BulkInsertTestScreenshots] AS' 
END
GO


--------------------------------------------------------------------------------
-- 6. Create the updated stored procedures
--------------------------------------------------------------------------------

-- Bulk insert screenshots
ALTER PROCEDURE [aut].[usp_BulkInsertTestScreenshots]
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
/****** Object:  StoredProcedure [aut].[usp_BulkRunTestCasesNow]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_BulkRunTestCasesNow]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_BulkRunTestCasesNow] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_BulkRunTestCasesNow]
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
/****** Object:  StoredProcedure [aut].[usp_BulkScheduleTestCases]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_BulkScheduleTestCases]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_BulkScheduleTestCases] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_BulkScheduleTestCases]
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
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ChangePassword]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ChangePassword] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_ChangePassword]
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
/****** Object:  StoredProcedure [aut].[usp_CountAutomationDataForSection]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_CountAutomationDataForSection]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_CountAutomationDataForSection] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_CountAutomationDataForSection]
    @SectionID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM aut.AutomationData WHERE SectionID = @SectionID;
END

GO
/****** Object:  StoredProcedure [aut].[usp_CreateOrUpdateAssignmentWithTestCases]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_CreateOrUpdateAssignmentWithTestCases]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_CreateOrUpdateAssignmentWithTestCases] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_CreateOrUpdateAssignmentWithTestCases]
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
/****** Object:  StoredProcedure [aut].[usp_CreateRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_CreateRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_CreateRelease] AS' 
END
GO

/*============================================================================
  7. STORED PROCEDURES  (create/alter to match the new schema)
============================================================================*/

-- 7.1 usp_CreateRelease  (adds Version, EnvironmentId, ReleaseFolderPath)
ALTER   PROCEDURE [aut].[usp_CreateRelease]
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
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_CreateUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_CreateUser] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_CreateUser]
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
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationData]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteAutomationData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteAutomationData] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_DeleteAutomationData] @SectionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE [aut].[AutomationData]
	WHERE SectionID = @SectionID
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSection]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteAutomationDataSection]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteAutomationDataSection] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_DeleteAutomationDataSection] @SectionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DELETE [aut].[AutomationDataSections]
	WHERE SectionID = @SectionID
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteAutomationDataSectionCascade]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteAutomationDataSectionCascade]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteAutomationDataSectionCascade] AS' 
END
GO

-- Cascade delete for a Section WITH its saved test data - used only when the user
-- explicitly confirms "delete the section AND its data" from the new UI (the default
-- delete path above still blocks if data exists). Runs both deletes in one
-- transaction so a failure partway through can't leave the section gone but its data
-- orphaned (or vice versa).
ALTER   PROCEDURE [aut].[usp_DeleteAutomationDataSectionCascade]
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
/****** Object:  StoredProcedure [aut].[usp_DeleteRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteRelease] AS' 
END
GO

-- 7.4c usp_DeleteRelease  (compensating delete; used ONLY to roll back a Release
-- row when the immediately-following physical folder creation fails, so a
-- failed Create never falsely reports success. Cascades to ReleaseDLL,
-- ReleaseNotification, ReleaseSignOff via existing FK ON DELETE CASCADE.)
ALTER   PROCEDURE [aut].[usp_DeleteRelease]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[Release] WHERE ReleaseId = @ReleaseId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestScreenshotById]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteTestScreenshotById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteTestScreenshotById] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_DeleteTestScreenshotById]
    @ID INT
AS
BEGIN
    DELETE FROM [aut].[TestScreenshots]
    WHERE ID = @ID;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_DeleteUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_DeleteUser] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_DeleteUser]
    @UserID INT
AS
BEGIN
    DELETE FROM [aut].[User]
    WHERE UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentCreate]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentCreate]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentCreate] AS' 
END
GO

-- 1b. usp_EnvironmentCreate/Update/GetAll/GetById - accept/return EnvironmentUrl +
-- RequiresAuthentication. Bodies otherwise unchanged from their current live definitions.

ALTER   PROCEDURE [aut].[usp_EnvironmentCreate]
(
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @CreatedBy INT,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1
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

    INSERT INTO aut.Environment
    (
        EnvironmentName,
        Description,
        IsActive,
        CreatedBy,
        EnvironmentUrl,
        RequiresAuthentication
    )
    VALUES
    (
        @EnvironmentName,
        @Description,
        1,
        @CreatedBy,
        @EnvironmentUrl,
        @RequiresAuthentication
    );

    SELECT SCOPE_IDENTITY();
END

GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetAll]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentGetAll]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentGetAll] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_EnvironmentGetAll]
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
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetById]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentGetById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentGetById] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_EnvironmentGetById]
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
/****** Object:  StoredProcedure [aut].[usp_EnvironmentHardDelete]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentHardDelete]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentHardDelete] AS' 
END
GO

-- 4. usp_EnvironmentHardDelete - guarded against in-use environments instead of an
-- unconditional DELETE that would otherwise surface a raw FK-violation error.
ALTER   PROCEDURE [aut].[usp_EnvironmentHardDelete]
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
/****** Object:  StoredProcedure [aut].[usp_EnvironmentSoftDelete]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentSoftDelete]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentSoftDelete] AS' 
END
GO

-- 3. usp_EnvironmentSoftDelete - now also stamps ModifiedBy.
ALTER   PROCEDURE [aut].[usp_EnvironmentSoftDelete]
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
/****** Object:  StoredProcedure [aut].[usp_EnvironmentUpdate]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_EnvironmentUpdate]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_EnvironmentUpdate] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_EnvironmentUpdate]
(
    @EnvironmentId INT,
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @IsActive BIT,
    @ModifiedBy INT = NULL,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        EnvironmentName = @EnvironmentName,
        Description = @Description,
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME(),
        EnvironmentUrl = @EnvironmentUrl,
        RequiresAuthentication = @RequiresAuthentication
    WHERE EnvironmentId = @EnvironmentId;
END

GO
/****** Object:  StoredProcedure [aut].[usp_ForgotPassword]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ForgotPassword]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ForgotPassword] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_ForgotPassword]
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
/****** Object:  StoredProcedure [aut].[usp_get_AutomationDataSection]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_get_AutomationDataSection]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_get_AutomationDataSection] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_get_AutomationDataSection]   
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
/****** Object:  StoredProcedure [aut].[usp_get_AutomationFlowNames]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_get_AutomationFlowNames]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_get_AutomationFlowNames] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_get_AutomationFlowNames]   
  
AS  
BEGIN  
SET NOCOUNT ON;  
  
Select Distinct FlowName from [aut].[AutomationDataSections]
   
END  
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllAssignedTestCasesForRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAllAssignedTestCasesForRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAllAssignedTestCasesForRelease] AS' 
END
GO

-- 7.13 usp_GetAllAssignedTestCasesForRelease  (NEW - same shape as
--      usp_GetAllAssignedTestCasesInLibrary but spans every library tied to a Release,
--      for the Dashboard's Release-level "Individual Test Case Results" grid)
ALTER   PROCEDURE [aut].[usp_GetAllAssignedTestCasesForRelease]
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
/****** Object:  StoredProcedure [aut].[usp_GetAllAssignedTestCasesInLibrary]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAllAssignedTestCasesInLibrary]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAllAssignedTestCasesInLibrary] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_GetAllAssignedTestCasesInLibrary]
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
/****** Object:  StoredProcedure [aut].[usp_GetAllRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAllRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAllRelease] AS' 
END
GO

-- 7.3 usp_GetAllRelease  (join Environment, test summary)
-- NOTE: DLL readiness is filesystem state (DLLs are placed by the existing
-- controlled build/deploy process, not tracked in the database), so it is
-- computed by the application layer (IReleaseReadinessService), not here.
ALTER   PROCEDURE [aut].[usp_GetAllRelease]
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
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAllUsers]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAllUsers] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_GetAllUsers]
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignedTestCasesForLibraryAndRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAssignedTestCasesForLibraryAndRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndRelease] AS' 
END
GO

-- 7.11 usp_GetAssignedTestCasesForLibraryAndRelease  (NEW - duplicate-assignment check
--      scoped by Library + Release instead of Library + Environment text, so the same
--      test case can be assigned in two different Releases that share an Environment,
--      but not twice within the same Release)
ALTER   PROCEDURE [aut].[usp_GetAssignedTestCasesForLibraryAndRelease]
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignmentExecutionLogs]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAssignmentExecutionLogs]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAssignmentExecutionLogs] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetAssignmentExecutionLogs]
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
/****** Object:  StoredProcedure [aut].[usp_GetAssignmentReleaseLifecycle]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAssignmentReleaseLifecycle]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAssignmentReleaseLifecycle] AS' 
END
GO

-- 7.14 usp_GetAssignmentReleaseLifecycle  (NEW - lets the execution queue endpoints
--      block Run Now/Schedule once the assignment's linked Release is no longer Active)
ALTER   PROCEDURE [aut].[usp_GetAssignmentReleaseLifecycle]
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationData]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAutomationData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAutomationData] AS' 
END
GO

/*----------------------------------------------------------------------------
  3. Stored procedures
----------------------------------------------------------------------------*/

-- usp_GetAutomationData - now also filters by EnvironmentId
ALTER   PROCEDURE [aut].[usp_GetAutomationData]
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataByFlowName]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAutomationDataByFlowName]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAutomationDataByFlowName] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_GetAutomationDataByFlowName] @FlowName NVARCHAR(150) = NULL
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationDataSection]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAutomationDataSection]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAutomationDataSection] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetAutomationDataSection]
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
/****** Object:  StoredProcedure [aut].[usp_GetAutomationFlowNames]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetAutomationFlowNames]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetAutomationFlowNames] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_GetAutomationFlowNames]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT FlowName
	FROM [aut].[AutomationDataSections] WITH (NOLOCK)
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetFilteredUsers]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetFilteredUsers]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetFilteredUsers] AS' 
END
GO
/**********************************************************************************************************
Project Name: OHPNM - Automation Portal
SP Name: aut.[usp_GetFilteredUsers]
Purpose: This sp is used to get users based on search filters
Initial Creator: Shikha Aggarwal
Initial Creation Date: 15-Oct-2025
***********************************************************************************************************/

ALTER PROCEDURE [aut].[usp_GetFilteredUsers]
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
/****** Object:  StoredProcedure [aut].[usp_GetPendingExecutionQueues]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetPendingExecutionQueues]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetPendingExecutionQueues] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_GetPendingExecutionQueues]
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
        Q.QueueStatus,
        Q.ExecutionDateTime

    FROM aut.TestCaseExecutionQueue Q
    INNER JOIN aut.AssignedTestCases ATC
        ON Q.AssignmentTestCaseId = ATC.AssignmentTestCaseId
    INNER JOIN aut.TestCaseAssignment TCA
        ON ATC.AssignmentId = TCA.AssignmentId

    WHERE
        Q.QueueStatus = 'Queued'
        OR (Q.QueueStatus = 'Scheduled' AND Q.ExecutionDateTime <= GETDATE())

    ORDER BY
        Q.CreatedDate ASC;
END

GO
/****** Object:  StoredProcedure [aut].[usp_GetPriorityStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetPriorityStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetPriorityStatus] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetPriorityStatus]
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
/****** Object:  StoredProcedure [aut].[usp_GetReleaseById]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetReleaseById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetReleaseById] AS' 
END
GO

-- 7.4 usp_GetReleaseById  (same shape as GetAll, single release)
ALTER   PROCEDURE [aut].[usp_GetReleaseById]
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
/****** Object:  StoredProcedure [aut].[usp_GetReleaseExecutionLogs]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetReleaseExecutionLogs]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetReleaseExecutionLogs] AS' 
END
GO

-- 7.10 usp_GetReleaseExecutionLogs  (ReleaseId-based now that ReleaseId is NOT NULL and
--      reliable, replacing the old @ReleaseName filter which actually matched the
--      historically-misnamed Library-name text column, not the real Release name.
--      Also now selects LogId, previously missing.)
ALTER   PROCEDURE [aut].[usp_GetReleaseExecutionLogs]
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
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotById]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetScreenshotById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetScreenshotById] AS' 
END
GO

-- Get screenshot by ID
ALTER PROCEDURE [aut].[usp_GetScreenshotById]
    @ID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ID, AssignmentTestCaseId ,Caption, Screenshot, TakenAt
    FROM aut.TestScreenshots
    WHERE ID = @ID;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetScreenshotsByAssignmentTestCaseId]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetScreenshotsByAssignmentTestCaseId]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetScreenshotsByAssignmentTestCaseId] AS' 
END
GO

-- Get all screenshots by QueueId and optionally MethodName
ALTER PROCEDURE [aut].[usp_GetScreenshotsByAssignmentTestCaseId]
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
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseAssignmentsByUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetTestCaseAssignmentsByUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetTestCaseAssignmentsByUser] AS' 
END
GO

-- 7.9 usp_GetTestCaseAssignmentsByUser  (additionally return ReleaseId/EnvironmentId)
ALTER   PROCEDURE [aut].[usp_GetTestCaseAssignmentsByUser]
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
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseExecutionLogs]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetTestCaseExecutionLogs]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetTestCaseExecutionLogs] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetTestCaseExecutionLogs]
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
/****** Object:  StoredProcedure [aut].[usp_GetTestCasesByAssignmentNameAndUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetTestCasesByAssignmentNameAndUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetTestCasesByAssignmentNameAndUser] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetTestCasesByAssignmentNameAndUser]
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
/****** Object:  StoredProcedure [aut].[usp_GetTimeZones]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetTimeZones]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetTimeZones] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetTimeZones]
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
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetUserById]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetUserById] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_GetUserById]
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
/****** Object:  StoredProcedure [aut].[usp_GetUserByUsername]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetUserByUsername]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetUserByUsername] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_GetUserByUsername]
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
/****** Object:  StoredProcedure [aut].[usp_GetUsernameByEmail]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetUsernameByEmail]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetUsernameByEmail] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetUsernameByEmail]
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
/****** Object:  StoredProcedure [aut].[usp_GetUserRoles]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetUserRoles]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetUserRoles] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_GetUserRoles]
AS
BEGIN
    SELECT 
        RoleID,
		RoleName
    FROM [aut].[UserRole]    
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserStatuses]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_GetUserStatuses]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_GetUserStatuses] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_GetUserStatuses]
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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationData]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_InsertAutomationData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_InsertAutomationData] AS' 
END
GO

-- usp_InsertAutomationData - now stores EnvironmentId
ALTER   PROCEDURE [aut].[usp_InsertAutomationData]
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
/****** Object:  StoredProcedure [aut].[usp_InsertAutomationDataSection]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_InsertAutomationDataSection]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_InsertAutomationDataSection] AS' 
END
GO

ALTER PROC [aut].[usp_InsertAutomationDataSection] (
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
/****** Object:  StoredProcedure [aut].[usp_InsertTestScreenshot]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_InsertTestScreenshot]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_InsertTestScreenshot] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_InsertTestScreenshot]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserCreate]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserCreate]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserCreate] AS' 
END
GO

-- 4. Stored procedures

ALTER   PROCEDURE [aut].[usp_LoginUserCreate]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetByEnvironment]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserGetByEnvironment]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserGetByEnvironment] AS' 
END
GO

-- Used by both the Login Users management screen and the Run Now/Schedule dropdowns -
-- never returns EncryptedPassword.
ALTER   PROCEDURE [aut].[usp_LoginUserGetByEnvironment]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetByEnvironmentAndPortalUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserGetByEnvironmentAndPortalUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserGetByEnvironmentAndPortalUser] AS' 
END
GO

-- Same shape as usp_LoginUserGetByEnvironment, plus an owner filter. Used only by the
-- self-service Credential Configuration screen - Run Now/Schedule keep using the
-- existing, unfiltered usp_LoginUserGetByEnvironment (they need to see the credential
-- the person running/scheduling the test picked for themselves via the same ownership
-- filter, resolved by the API from the caller's own JWT - see LoginUserController).
ALTER   PROCEDURE [aut].[usp_LoginUserGetByEnvironmentAndPortalUser]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserGetCredentials]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserGetCredentials]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserGetCredentials] AS' 
END
GO

-- The only procedure that ever returns EncryptedPassword - a plain lookup by primary
-- key, called only by an isolated test process's service JWT (see LoginUserController).
ALTER   PROCEDURE [aut].[usp_LoginUserGetCredentials]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserHardDelete]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserHardDelete]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserHardDelete] AS' 
END
GO

-- Still guarded like usp_EnvironmentHardDelete (a LoginUserId already used by a real
-- queued/scheduled/executed run can't be hard-deleted without violating the FK) - now
-- also ownership-checked.
ALTER   PROCEDURE [aut].[usp_LoginUserHardDelete]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserResolveByRole]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserResolveByRole]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserResolveByRole] AS' 
END
GO

-- Resolves the shared "default" login user for a given Environment+Role, used only by
-- BaseFeatureFixture.LoginByProfile's mid-test role-switch (e.g. TC.Registration logging
-- in as a different role partway through a test) - a genuinely different use case from
-- the initial Run Now/Schedule login (an unattended in-test call, not a human picking
-- from a dropdown), so a role-keyed lookup is appropriate here specifically. Returns the
-- most-recently-created active match for that Environment+Role, or no rows if none
-- configured - caller falls back to a clear failure, never silently to hard-coded data.
ALTER   PROCEDURE [aut].[usp_LoginUserResolveByRole]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserSoftDelete]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserSoftDelete]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserSoftDelete] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_LoginUserSoftDelete]
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
/****** Object:  StoredProcedure [aut].[usp_LoginUserUpdate]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_LoginUserUpdate]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_LoginUserUpdate] AS' 
END
GO

-- Ownership-enforced: @PortalUserId must match the row's own PortalUserId, or the
-- UPDATE affects 0 rows. A pre-existing row with PortalUserId IS NULL (e.g. old seed/
-- verification data) matches nobody - intentionally not self-service-editable.
ALTER   PROCEDURE [aut].[usp_LoginUserUpdate]
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
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_RegisterUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_RegisterUser] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_RegisterUser]
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
/****** Object:  StoredProcedure [aut].[usp_Release_SetFolderPath]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_Release_SetFolderPath]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_Release_SetFolderPath] AS' 
END
GO

-- 7.4b usp_Release_SetFolderPath  (store the resolved folder path after physical creation)
ALTER   PROCEDURE [aut].[usp_Release_SetFolderPath]
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
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_Add]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ReleaseNotification_Add]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ReleaseNotification_Add] AS' 
END
GO

-- 7.7 ReleaseNotification stored procedures
ALTER   PROCEDURE [aut].[usp_ReleaseNotification_Add]
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
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_GetByRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ReleaseNotification_GetByRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ReleaseNotification_GetByRelease] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_ReleaseNotification_GetByRelease]
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
/****** Object:  StoredProcedure [aut].[usp_ReleaseNotification_MarkSent]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ReleaseNotification_MarkSent]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ReleaseNotification_MarkSent] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_ReleaseNotification_MarkSent]
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
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ReleaseSignOff]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ReleaseSignOff] AS' 
END
GO

-- 7.6 usp_ReleaseSignOff  (only after all assigned tests are terminal; approve/reject + history)
ALTER   PROCEDURE [aut].[usp_ReleaseSignOff]
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
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff_GetByRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ReleaseSignOff_GetByRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ReleaseSignOff_GetByRelease] AS' 
END
GO

-- 7.6b usp_ReleaseSignOff_GetByRelease  (sign-off history for a release)
ALTER   PROCEDURE [aut].[usp_ReleaseSignOff_GetByRelease]
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
/****** Object:  StoredProcedure [aut].[usp_ResetPassword]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ResetPassword]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ResetPassword] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_ResetPassword]
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
/****** Object:  StoredProcedure [aut].[usp_ScheduleSingleTestCase]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_ScheduleSingleTestCase]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_ScheduleSingleTestCase] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_ScheduleSingleTestCase]
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
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_SetUserActiveStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_SetUserActiveStatus] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_SetUserActiveStatus]
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
/****** Object:  StoredProcedure [aut].[usp_SingleRunTestCaseNow]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_SingleRunTestCaseNow]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_SingleRunTestCaseNow] AS' 
END
GO

-- 5. Queue insert procs + pending-queue getter: thread LoginUserId (optional) through.

ALTER   PROCEDURE [aut].[usp_SingleRunTestCaseNow]
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
/****** Object:  StoredProcedure [aut].[usp_TestExecutionNotification_Add]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_TestExecutionNotification_Add]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_TestExecutionNotification_Add] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_TestExecutionNotification_Add]
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
/****** Object:  StoredProcedure [aut].[usp_TestExecutionNotification_MarkSent]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_TestExecutionNotification_MarkSent]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_TestExecutionNotification_MarkSent] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_TestExecutionNotification_MarkSent]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAssignedTestCaseStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateAssignedTestCaseStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateAssignedTestCaseStatus] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_UpdateAssignedTestCaseStatus]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationData]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateAutomationData]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateAutomationData] AS' 
END
GO
ALTER PROCEDURE [aut].[usp_UpdateAutomationData]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateAutomationDataSections]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateAutomationDataSections]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateAutomationDataSections] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_UpdateAutomationDataSections] @SectionName NVARCHAR(500) = NULL
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
/****** Object:  StoredProcedure [aut].[usp_UpdateLastLogin]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateLastLogin]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateLastLogin] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_UpdateLastLogin]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateQueueStatus]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateQueueStatus]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateQueueStatus] AS' 
END
GO
ALTER   PROCEDURE [aut].[usp_UpdateQueueStatus]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateRelease]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateRelease]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateRelease] AS' 
END
GO

-- 7.2 usp_UpdateRelease  (adds Version/EnvironmentId/ReleaseFolderPath/ModifiedBy)
ALTER   PROCEDURE [aut].[usp_UpdateRelease]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateUser]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateUser] AS' 
END
GO

ALTER PROCEDURE [aut].[usp_UpdateUser]
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
/****** Object:  StoredProcedure [aut].[usp_UpdateUserProfile]    Script Date: 23-09-2026 10:30:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[usp_UpdateUserProfile]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [aut].[usp_UpdateUserProfile] AS' 
END
GO

ALTER   PROCEDURE [aut].[usp_UpdateUserProfile]
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



GO
IF NOT EXISTS (SELECT 1 FROM [aut].[PriorityStatus] WHERE [PriorityID] = 1)
BEGIN
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (1, N'Tier 1')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[PriorityStatus] WHERE [PriorityID] = 2)
BEGIN
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (2, N'Tier 2')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[PriorityStatus] WHERE [PriorityID] = 3)
BEGIN
INSERT [aut].[PriorityStatus] ([PriorityID], [PriorityName]) VALUES (3, N'Tier 3')
END
GO
SET IDENTITY_INSERT [aut].[TimeZone] ON 
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[TimeZone] WHERE [TimeZoneID] = 1)
BEGIN
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (1, N'UTC', 0, N'Coordinated Universal Time')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[TimeZone] WHERE [TimeZoneID] = 2)
BEGIN
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (2, N'Eastern Standard Time', -300, N'UTC-5')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[TimeZone] WHERE [TimeZoneID] = 3)
BEGIN
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (3, N'Central Standard Time', -360, N'UTC-6')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[TimeZone] WHERE [TimeZoneID] = 4)
BEGIN
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (4, N'India Standard Time', 330, N'UTC+5:30')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[TimeZone] WHERE [TimeZoneID] = 5)
BEGIN
INSERT [aut].[TimeZone] ([TimeZoneID], [TimeZoneName], [UTCOffsetMinutes], [Description]) VALUES (5, N'Pacific Standard Time', -480, N'UTC-8')
END
GO
SET IDENTITY_INSERT [aut].[TimeZone] OFF
GO
SET IDENTITY_INSERT [aut].[UserRole] ON 
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserRole] WHERE [RoleID] = 1)
BEGIN
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (1, N'Admin')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserRole] WHERE [RoleID] = 2)
BEGIN
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (2, N'Tester')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserRole] WHERE [RoleID] = 3)
BEGIN
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (3, N'Manager')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserRole] WHERE [RoleID] = 4)
BEGIN
INSERT [aut].[UserRole] ([RoleID], [RoleName]) VALUES (4, N'Viewer')
END
GO
SET IDENTITY_INSERT [aut].[UserRole] OFF
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserStatus] WHERE [StatusID] = 1)
BEGIN
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (1, N'Active')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserStatus] WHERE [StatusID] = 2)
BEGIN
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (2, N'Suspended')
END
GO
IF NOT EXISTS (SELECT 1 FROM [aut].[UserStatus] WHERE [StatusID] = 3)
BEGIN
INSERT [aut].[UserStatus] ([StatusID], [StatusName]) VALUES (3, N'Pending')
END
GO
