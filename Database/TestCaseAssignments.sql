
/****** Object:  UserDefinedTableType [aut].[TestCaseAssignmentType]    Script Date: 26-09-2025 00:53:48 ******/
CREATE TYPE [aut].[TestCaseAssignmentType] AS TABLE(
	[UserId] [int] NOT NULL,
	[LibraryName] [nvarchar](max) NOT NULL,
	[ClassName] [nvarchar](max) NOT NULL,
	[MethodName] [nvarchar](max) NOT NULL,
	[AssignedBy] [int] NULL
)
GO
/****** Object:  Table [aut].[TestCaseAssignments]    Script Date: 26-09-2025 00:53:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [aut].[TestCaseAssignments](
	[AssignmentId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[LibraryName] [nvarchar](max) NOT NULL,
	[ClassName] [nvarchar](max) NOT NULL,
	[MethodName] [nvarchar](max) NOT NULL,
	[AssignedOn] [datetime] NOT NULL,
	[AssignedBy] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[AssignmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [aut].[TestCaseAssignments] ADD  DEFAULT (getdate()) FOR [AssignedOn]
GO
ALTER TABLE [aut].[TestCaseAssignments]  WITH CHECK ADD  CONSTRAINT [FK_TestCaseAssignments_User] FOREIGN KEY([UserId])
REFERENCES [aut].[User] ([UserID])
GO
ALTER TABLE [aut].[TestCaseAssignments] CHECK CONSTRAINT [FK_TestCaseAssignments_User]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteTestCaseAssignmentsByUser]    Script Date: 26-09-2025 00:53:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_DeleteTestCaseAssignmentsByUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [aut].[TestCaseAssignments]
    WHERE UserId = @UserId;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_GetTestCaseAssignmentsByUser]    Script Date: 26-09-2025 00:53:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetTestCaseAssignmentsByUser]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        AssignmentId,
        UserId,
        LibraryName,
        ClassName,
        MethodName,
        AssignedOn,
        AssignedBy
    FROM [aut].[TestCaseAssignments]
    WHERE UserId = @UserId
    ORDER BY LibraryName, ClassName, MethodName;
END;
GO
/****** Object:  StoredProcedure [aut].[usp_SyncTestCaseAssignments]    Script Date: 26-09-2025 00:53:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [aut].[usp_SyncTestCaseAssignments]
    @Assignments [aut].[TestCaseAssignmentType] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT;
    SELECT TOP 1 @UserId = UserId FROM @Assignments;

    IF @UserId IS NULL
    BEGIN
        RAISERROR('No UserId provided in assignments', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;

    -- 1. Delete assignments that are no longer in the input
    DELETE FROM [aut].[TestCaseAssignments]
    WHERE UserId = @UserId
      AND NOT EXISTS (
          SELECT 1
          FROM @Assignments a
          WHERE a.UserId = [aut].[TestCaseAssignments].UserId
            AND a.LibraryName = [aut].[TestCaseAssignments].LibraryName
            AND a.ClassName = [aut].[TestCaseAssignments].ClassName
            AND a.MethodName = [aut].[TestCaseAssignments].MethodName
      );

    -- 2. Insert new assignments
    INSERT INTO [aut].[TestCaseAssignments]
        (UserId, LibraryName, ClassName, MethodName, AssignedOn, AssignedBy)
    SELECT 
        a.UserId,
        a.LibraryName,
        a.ClassName,
        a.MethodName,
        GETDATE(),
        a.AssignedBy
    FROM @Assignments a
    WHERE NOT EXISTS (
        SELECT 1
        FROM [aut].[TestCaseAssignments] t
        WHERE t.UserId = a.UserId
          AND t.LibraryName = a.LibraryName
          AND t.ClassName = a.ClassName
          AND t.MethodName = a.MethodName
    );

    COMMIT TRANSACTION;
END;
GO
