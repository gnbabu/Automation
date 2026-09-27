-- AuditLog_Migration.sql
-- Idempotent (safe to re-run). Adds a persistent, queryable audit trail - today there is
-- no record anywhere of who created/renamed/deleted a Section/Flow, changed an
-- Environment, etc. A few endpoints (EnvironmentController.Update/SoftDelete,
-- LoginUserController) capture a ModifiedBy/DeletedBy column on the row *itself*, but
-- that is overwritten on the next change and lost entirely once the row is deleted -
-- there is no history. This is a new, dedicated, generic table (not an extension of any
-- existing entity table) so one screen/API can show activity across every module.
--
-- EntityId is nullable because a "Flow" has no single numeric id of its own (it's just
-- the distinct FlowName value on aut.AutomationDataSections rows) - Flow-level actions
-- (e.g. deleting a whole flow, which fans out to per-section deletes client-side) are
-- still logged with EntityType='Flow', EntityId=NULL, EntityName=<flow name>.
--
-- ActorUserId is nullable for system/background actions with no logged-in user in
-- context at all (e.g. RecurringScheduleWorker auto-pausing a schedule when its release
-- completes/is rejected) - ActorUserName is always populated ('System' for those cases)
-- so the log reads correctly without a join, and remains readable even if the acting
-- user is later deleted/renamed (a snapshot, not a live FK-dependent lookup).

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('aut.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[AuditLog](
        [AuditLogId]    INT IDENTITY(1,1) NOT NULL,
        [EntityType]    NVARCHAR(50)  NOT NULL,
        [EntityId]      INT           NULL,
        [EntityName]    NVARCHAR(200) NOT NULL,
        [Action]        NVARCHAR(30)  NOT NULL,
        [ActorUserId]   INT           NULL,
        [ActorUserName] NVARCHAR(100) NOT NULL,
        [Details]       NVARCHAR(MAX) NULL,
        [CreatedOn]     DATETIME2(7)  NOT NULL CONSTRAINT [DF_AuditLog_CreatedOn] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([AuditLogId] ASC)
        -- Deliberately no FK to aut.User for ActorUserId - a user can be hard-deleted
        -- later and the audit trail must still read correctly (ActorUserName is the
        -- durable snapshot; ActorUserId is best-effort for cross-referencing while the
        -- user still exists).
    );
    CREATE NONCLUSTERED INDEX [IX_AuditLog_EntityType_EntityId]
        ON aut.[AuditLog] ([EntityType], [EntityId], [CreatedOn] DESC);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_CreatedOn]
        ON aut.[AuditLog] ([CreatedOn] DESC);
    CREATE NONCLUSTERED INDEX [IX_AuditLog_ActorUserId]
        ON aut.[AuditLog] ([ActorUserId], [CreatedOn] DESC);
    PRINT 'Created aut.AuditLog';
END
GO

IF OBJECT_ID('aut.usp_AuditLog_Insert', 'P') IS NOT NULL
    DROP PROCEDURE aut.usp_AuditLog_Insert;
GO
CREATE PROCEDURE aut.usp_AuditLog_Insert
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

IF OBJECT_ID('aut.usp_AuditLog_GetPaged', 'P') IS NOT NULL
    DROP PROCEDURE aut.usp_AuditLog_GetPaged;
GO
CREATE PROCEDURE aut.usp_AuditLog_GetPaged
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

IF OBJECT_ID('aut.usp_AuditLog_GetDistinctEntityTypes', 'P') IS NOT NULL
    DROP PROCEDURE aut.usp_AuditLog_GetDistinctEntityTypes;
GO
-- Powers the Entity Type filter dropdown on the Activity Log screen without hard-coding
-- the list on the frontend - reflects exactly what's actually been logged so far.
CREATE PROCEDURE aut.usp_AuditLog_GetDistinctEntityTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT [EntityType] FROM aut.[AuditLog] ORDER BY [EntityType];
END
GO

-- Small, additive lookup-by-id for aut.LoginUser - none of usp_LoginUserGetByEnvironment/
-- GetByEnvironmentAndPortalUser take just an id, and LoginUserController's Update/
-- SoftDelete/HardDelete endpoints only ever receive one - added so the audit log entries
-- for those actions can show a readable "TechAdmin (jdoe)" name instead of just an id.
-- Same shape as usp_LoginUserGetByEnvironmentAndPortalUser; does not change any existing
-- procedure's behavior.
IF OBJECT_ID('aut.usp_LoginUserGetById', 'P') IS NOT NULL
    DROP PROCEDURE aut.usp_LoginUserGetById;
GO
CREATE PROCEDURE aut.usp_LoginUserGetById
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

-- Same rationale as usp_LoginUserGetById above: AutomationController's UpdateAutomationData
-- endpoint (Test Data Management's field editor "Save"/"Update") only ever receives the
-- row's own @ID + new @TestContent - never SectionID/UserID/EnvironmentId - so there was
-- no way for the audit-log instrumentation to know which section/user/environment a given
-- update belongs to, or to diff the old TestContent against the new one, without this.
IF OBJECT_ID('aut.usp_GetAutomationDataById', 'P') IS NOT NULL
    DROP PROCEDURE aut.usp_GetAutomationDataById;
GO
CREATE PROCEDURE aut.usp_GetAutomationDataById
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
