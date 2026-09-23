-- Notification_Center_Migration.sql
-- Idempotent (safe to re-run). Adds a persistent, per-user in-app notification feed -
-- today, release-activated/ready-to-activate/signed-off and scheduled-run-failure events
-- are outbound-email-only (ReleaseNotificationService/TestExecutionNotificationService),
-- with no cross-cutting personal record if a user misses the email. This is a new,
-- dedicated table - NOT an extension of aut.ReleaseNotification/aut.TestExecutionNotification,
-- which remain exactly what they are today (email-delivery audit logs keyed by
-- Status='Sent'/'Failed', not read/unread); those two tables' existing behavior/screens
-- are untouched by this migration.
--
-- Recipients mirror exactly who already gets emailed today for each event type - no new
-- "who gets notified" policy is introduced here.

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('aut.Notification', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[Notification](
        [NotificationId]   INT IDENTITY(1,1) NOT NULL,
        [UserId]           INT NOT NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [Title]            NVARCHAR(200) NOT NULL,
        [Message]          NVARCHAR(500) NULL,
        [LinkUrl]          NVARCHAR(300) NULL,
        [SourceType]       NVARCHAR(30) NOT NULL,
        [SourceId]         INT NULL,
        [IsRead]           BIT NOT NULL CONSTRAINT [DF_Notification_IsRead] DEFAULT (0),
        [ReadOn]           DATETIME2(7) NULL,
        [CreatedOn]        DATETIME2(7) NOT NULL CONSTRAINT [DF_Notification_CreatedOn] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED ([NotificationId] ASC),
        CONSTRAINT [FK_Notification_User] FOREIGN KEY ([UserId])
            REFERENCES aut.[User] ([UserID])
    );
    CREATE NONCLUSTERED INDEX [IX_Notification_UserId_IsRead_CreatedOn]
        ON aut.[Notification] ([UserId], [IsRead], [CreatedOn] DESC);
    PRINT 'Created aut.Notification';
END
GO

CREATE OR ALTER PROCEDURE aut.usp_Notification_Add
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

-- Returns all of a user's notifications, newest first - paged client-side by the
-- frontend's shared app-data-grid component, matching every other list page in this
-- app (Users/Environments/Releases/etc. all use pagingMode: 'client'; no page anywhere
-- uses server-side paging today, so introducing a novel OFFSET/FETCH-based SP here
-- would be inconsistent with the rest of the codebase for no real benefit at this
-- data volume).
CREATE OR ALTER PROCEDURE aut.usp_Notification_GetByUser
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

CREATE OR ALTER PROCEDURE aut.usp_Notification_GetUnreadCount
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

-- Ownership-checked (WHERE ... AND UserId = @UserId) so one user can never mark
-- another user's notification read via a guessed/enumerated NotificationId.
CREATE OR ALTER PROCEDURE aut.usp_Notification_MarkRead
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

CREATE OR ALTER PROCEDURE aut.usp_Notification_MarkAllRead
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

-- One-time backfill of existing historical data (confirmed via direct query: 91
-- aut.ReleaseNotification + 3 aut.TestExecutionNotification rows have a resolvable
-- RecipientUserId, spanning ~3 weeks - a modest, recent amount worth surfacing rather
-- than starting the new feed empty). Guarded so re-running this migration (e.g. as
-- part of a future full DB rebuild) never duplicates the backfilled rows. Rows with no
-- resolvable RecipientUserId (email-only recipients) are skipped - aut.Notification's
-- UserId is NOT NULL by design, since an in-app notification is meaningless without an
-- owning user. Backfilled rows are inserted as already-read (ReadOn = their original
-- SentOn/CreatedOn) since they were already delivered via email historically - this
-- avoids a misleading "94 unread" badge spike on day one; only genuinely new events
-- going forward start unread. CreatedOn is copied from the original row (not
-- SYSDATETIME()) so backfilled items sort correctly alongside new ones.
IF NOT EXISTS (SELECT 1 FROM aut.[Notification])
BEGIN
    INSERT INTO aut.[Notification] (UserId, NotificationType, Title, Message, LinkUrl, SourceType, SourceId, IsRead, ReadOn, CreatedOn)
    SELECT
        RecipientUserId,
        NotificationType,
        -- Legacy Message is literally "Notify {username}: {subject}" (see
        -- ReleaseNotificationService.NotifyManagersAndAdminsAsync) - strip that prefix
        -- so backfilled titles read the same as freshly-created ones.
        CASE WHEN CHARINDEX(': ', Message) > 0
             THEN SUBSTRING(Message, CHARINDEX(': ', Message) + 2, 200)
             ELSE LEFT(Message, 200) END,
        NULL,
        '/release-management',
        'Release',
        ReleaseId,
        1,
        COALESCE(SentOn, CreatedOn),
        CreatedOn
    FROM aut.[ReleaseNotification]
    WHERE RecipientUserId IS NOT NULL;

    INSERT INTO aut.[Notification] (UserId, NotificationType, Title, Message, LinkUrl, SourceType, SourceId, IsRead, ReadOn, CreatedOn)
    SELECT
        RecipientUserId,
        NotificationType,
        CASE WHEN CHARINDEX(': ', Message) > 0
             THEN SUBSTRING(Message, CHARINDEX(': ', Message) + 2, 200)
             ELSE LEFT(Message, 200) END,
        NULL,
        '/test-case-execution-panel',
        'TestExecution',
        AssignmentTestCaseId,
        1,
        COALESCE(SentOn, CreatedOn),
        CreatedOn
    FROM aut.[TestExecutionNotification]
    WHERE RecipientUserId IS NOT NULL;

    PRINT 'Backfilled aut.Notification from ReleaseNotification/TestExecutionNotification';
END
GO
