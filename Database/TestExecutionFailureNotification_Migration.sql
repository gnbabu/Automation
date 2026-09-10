-- TestExecutionFailureNotification_Migration.sql
-- Idempotent (safe to re-run). Adds failure notification tracking for scheduled test
-- runs - mirrors aut.ReleaseNotification's exact shape/procs (usp_ReleaseNotification_
-- Add/MarkSent), used only for a genuinely different event (a Scheduled run finishing
-- as Failed, not a Release lifecycle event) - kept as its own table rather than
-- generalizing aut.ReleaseNotification, which is tied to ReleaseId specifically.
--
-- No automatic retry is implemented here - only a notification, so a human decides
-- whether to retry (see AGENTS.md). Recipients are the assigned user + all Admins,
-- resolved at send time by TestExecutionNotificationService (mirrors
-- ReleaseNotificationService's own Manager/Admin resolution).

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('aut.TestExecutionNotification', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[TestExecutionNotification](
        [TestExecutionNotificationId] INT IDENTITY(1,1) NOT NULL,
        [AssignmentTestCaseId]        INT NOT NULL,
        [NotificationType]            NVARCHAR(50) NOT NULL,
        [RecipientUserId]             INT NULL,
        [RecipientEmail]              NVARCHAR(255) NULL,
        [Status]                      NVARCHAR(30) NOT NULL CONSTRAINT [DF_TestExecutionNotification_Status] DEFAULT ('Pending'),
        [Message]                     NVARCHAR(500) NULL,
        [CreatedOn]                   DATETIME2(7) NOT NULL CONSTRAINT [DF_TestExecutionNotification_CreatedOn] DEFAULT (SYSDATETIME()),
        [SentOn]                      DATETIME2(7) NULL,
        CONSTRAINT [PK_TestExecutionNotification] PRIMARY KEY CLUSTERED ([TestExecutionNotificationId] ASC),
        CONSTRAINT [FK_TestExecutionNotification_AssignedTestCases] FOREIGN KEY ([AssignmentTestCaseId])
            REFERENCES aut.[AssignedTestCases] ([AssignmentTestCaseId])
    );
    CREATE NONCLUSTERED INDEX [IX_TestExecutionNotification_AssignmentTestCaseId]
        ON aut.[TestExecutionNotification] ([AssignmentTestCaseId]);
    PRINT 'Created aut.TestExecutionNotification';
END
GO

CREATE OR ALTER PROCEDURE aut.usp_TestExecutionNotification_Add
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

CREATE OR ALTER PROCEDURE aut.usp_TestExecutionNotification_MarkSent
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
