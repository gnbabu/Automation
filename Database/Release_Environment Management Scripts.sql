
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateRelease]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateRelease]
GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ReleaseSignOff]
GO
/****** Object:  StoredProcedure [aut].[usp_GetReleaseById]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetReleaseById]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllRelease]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAllRelease]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentUpdate]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentUpdate]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentSoftDelete]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentSoftDelete]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentHardDelete]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentHardDelete]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetById]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentGetById]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetAll]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentGetAll]
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentCreate]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_EnvironmentCreate]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateRelease]    Script Date: 17-01-2026 15:50:02 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_CreateRelease]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Environment]') AND type in (N'U'))
ALTER TABLE [aut].[Environment] DROP CONSTRAINT IF EXISTS [FK_Environment_CreatedBy]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Release]') AND type in (N'U'))
ALTER TABLE [aut].[Release] DROP CONSTRAINT IF EXISTS [DF__Release__Created__7849DB76]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Release]') AND type in (N'U'))
ALTER TABLE [aut].[Release] DROP CONSTRAINT IF EXISTS [DF__Release__SignOff__7755B73D]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Release]') AND type in (N'U'))
ALTER TABLE [aut].[Release] DROP CONSTRAINT IF EXISTS [DF__Release__IsActiv__76619304]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Release]') AND type in (N'U'))
ALTER TABLE [aut].[Release] DROP CONSTRAINT IF EXISTS [DF__Release__Release__756D6ECB]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Environment]') AND type in (N'U'))
ALTER TABLE [aut].[Environment] DROP CONSTRAINT IF EXISTS [DF__Environme__Creat__03BB8E22]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[aut].[Environment]') AND type in (N'U'))
ALTER TABLE [aut].[Environment] DROP CONSTRAINT IF EXISTS [DF__Environme__IsAct__02C769E9]
GO
/****** Object:  Table [aut].[Release]    Script Date: 17-01-2026 15:50:02 ******/
DROP TABLE IF EXISTS [aut].[Release]
GO
/****** Object:  Table [aut].[Environment]    Script Date: 17-01-2026 15:50:02 ******/
DROP TABLE IF EXISTS [aut].[Environment]
GO
/****** Object:  Table [aut].[Environment]    Script Date: 17-01-2026 15:50:02 ******/
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
PRIMARY KEY CLUSTERED 
(
	[EnvironmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [aut].[Release]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[Release](
	[ReleaseId] [int] IDENTITY(1,1) NOT NULL,
	[ReleaseName] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[ReleaseLifecycle] [nvarchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[SignOffStatus] [nvarchar](20) NOT NULL,
	[SignedOffBy] [nvarchar](100) NULL,
	[SignedOffOn] [datetime2](7) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedOn] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReleaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[Environment] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Draft') FOR [ReleaseLifecycle]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT ('Pending') FOR [SignOffStatus]
GO
ALTER TABLE [aut].[Release] ADD  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO
ALTER TABLE [aut].[Environment]  WITH CHECK ADD  CONSTRAINT [FK_Environment_CreatedBy] FOREIGN KEY([CreatedBy])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[Environment] CHECK CONSTRAINT [FK_Environment_CreatedBy]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateRelease]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [aut].[usp_CreateRelease]
(
    @ReleaseName NVARCHAR(100),
    @Description NVARCHAR(255),
    @CreatedBy NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [aut].[Release] WHERE ReleaseName = @ReleaseName)
    BEGIN
        RAISERROR ('Release already exists', 16, 1);
        RETURN;
    END

    INSERT INTO [aut].[Release]
    (
        ReleaseName,
        Description,
        ReleaseLifecycle,
        IsActive,
        SignOffStatus,
        CreatedBy
    )
    VALUES
    (
        @ReleaseName,
        @Description,
        'Draft',
        1,
        'Pending',
        @CreatedBy
    );

    SELECT SCOPE_IDENTITY();
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentCreate]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentCreate]
(
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @CreatedBy INT
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
        CreatedBy
    )
    VALUES
    (
        @EnvironmentName,
        @Description,
        1,
        @CreatedBy
    );

    SELECT SCOPE_IDENTITY();
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetAll]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentGetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnvironmentId,
        e.EnvironmentName,
        e.Description,
        e.IsActive,
        e.CreatedOn,

        u.UserID,
        u.UserName,
        u.Email
    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    ORDER BY e.CreatedOn DESC;
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentGetById]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentGetById]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.*,
        u.UserName,
        u.Email
    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    WHERE e.EnvironmentId = @EnvironmentId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentHardDelete]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentHardDelete]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM aut.Environment
    WHERE EnvironmentId = @EnvironmentId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentSoftDelete]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentSoftDelete]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        IsActive = 0,
        ModifiedOn = SYSDATETIME()
    WHERE EnvironmentId = @EnvironmentId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_EnvironmentUpdate]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_EnvironmentUpdate]
(
    @EnvironmentId INT,
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @IsActive BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        EnvironmentName = @EnvironmentName,
        Description = @Description,
        IsActive = @IsActive,
        ModifiedOn = SYSDATETIME()
    WHERE EnvironmentId = @EnvironmentId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllRelease]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetAllRelease]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [aut].[Release]
    ORDER BY CreatedOn DESC;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetReleaseById]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetReleaseById]
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [aut].Release
    WHERE ReleaseId = @ReleaseId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ReleaseSignOff]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [aut].[usp_ReleaseSignOff]
(
    @ReleaseId INT,
    @SignedOffBy NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Prevent sign-off if executions still running TODO
   

    UPDATE [aut].Release
    SET
        SignOffStatus = 'SignedOff',
        SignedOffBy = @SignedOffBy,
        SignedOffOn = SYSDATETIME(),
        ModifiedOn = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateRelease]    Script Date: 17-01-2026 15:50:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_UpdateRelease]
(
    @ReleaseId INT,
    @ReleaseName NVARCHAR(100),
    @Description NVARCHAR(255),
    @ReleaseLifecycle NVARCHAR(20),
    @IsActive BIT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[Release]
    SET
        ReleaseName = @ReleaseName,
        Description = @Description,
        ReleaseLifecycle = @ReleaseLifecycle,
        IsActive = @IsActive,
        ModifiedOn = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END
GO
