/*============================================================================
  OHPNM Release Management — Phase 2: Incremental Update Script
  Target database: MES_AUT_AI   Schema: aut

  Purpose:
    Bring an EXISTING database that already has Phase 1 applied
    (Release_Management_Migration_Phase1.sql) up to date with everything
    changed since, WITHOUT re-running the full Phase 1 script:

      1. usp_GetAllRelease / usp_GetReleaseById
         - Removed the ReleaseDLL-based "required/uploaded/validated/missing"
           DLL-count columns. DLLs are placed into the release folder by the
           existing controlled build/deployment process (not tracked in the
           database), so DLL readiness is now computed by the application
           layer (IReleaseReadinessService) via reflection over the release
           folder, not persisted in SQL.
      2. usp_Release_SetFolderPath (new)
         - Stores the resolved release folder path AFTER the row is inserted
           and the physical folder is created (the folder name embeds
           ReleaseId, so it can't be known before the insert).
      3. usp_DeleteRelease (new)
         - Compensating delete: rolls back a just-inserted Release row if
           physical folder creation fails (Create never falsely reports
           success). Also reused by the application's explicit "Delete"
           action for Draft-only releases. Cascades to ReleaseDLL,
           ReleaseNotification, ReleaseSignOff via existing FK
           ON DELETE CASCADE; blocked by SQL if TestCaseAssignment rows
           reference the release (no cascade there, by design).
      4. usp_ReleaseSignOff_GetByRelease (new)
         - Returns sign-off history (Approve/Reject/rework) for a release.

  This script does NOT change any table schema, drop any data, or touch
  the ReleaseDLL/ReleaseNotification/ReleaseSignOff tables themselves
  (they were already created by Phase 1). It only replaces stored procedure
  bodies (CREATE OR ALTER) and adds two new procedures.

  Safe to run multiple times (idempotent). Safe to run even if some or all
  of the above are already present (e.g. on a database that received a
  fresher copy of Phase 1) — CREATE OR ALTER simply reapplies the same
  definition.

  PREREQUISITE: aut.Release, aut.ReleaseDLL, aut.ReleaseNotification,
  aut.ReleaseSignOff, aut.TestCaseAssignment, aut.AssignedTestCases and
  aut.Environment must already exist (i.e. Phase 1 has been applied). If
  they don't, run Release_Management_Migration_Phase1.sql first — it is
  itself idempotent and safe to re-run instead of this file.
============================================================================*/

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT '== Phase 2 update starting ==';
GO

-- Prerequisite check
IF OBJECT_ID('aut.Release', 'U') IS NULL OR OBJECT_ID('aut.ReleaseSignOff', 'U') IS NULL
BEGIN
    RAISERROR('Prerequisite objects missing (aut.Release / aut.ReleaseSignOff). Run Release_Management_Migration_Phase1.sql first.', 16, 1);
    RETURN;
END
GO

/*============================================================================
  1. usp_GetAllRelease  (drop DLL-count columns; test summary only)
============================================================================*/
CREATE OR ALTER PROCEDURE aut.usp_GetAllRelease
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

/*============================================================================
  2. usp_GetReleaseById  (same shape as GetAll, single release)
============================================================================*/
CREATE OR ALTER PROCEDURE aut.usp_GetReleaseById
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

/*============================================================================
  3. usp_Release_SetFolderPath  (new)
============================================================================*/
CREATE OR ALTER PROCEDURE aut.usp_Release_SetFolderPath
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

/*============================================================================
  4. usp_DeleteRelease  (new — compensating delete AND explicit Draft-only delete)
============================================================================*/
CREATE OR ALTER PROCEDURE aut.usp_DeleteRelease
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[Release] WHERE ReleaseId = @ReleaseId;
END
GO

/*============================================================================
  5. usp_ReleaseSignOff_GetByRelease  (new — sign-off history for a release)
============================================================================*/
CREATE OR ALTER PROCEDURE aut.usp_ReleaseSignOff_GetByRelease
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

/*============================================================================
  Verification
============================================================================*/
PRINT '== Verifying updated/new procedures exist ==';
SELECT name FROM sys.procedures
WHERE name IN (
    'usp_GetAllRelease',
    'usp_GetReleaseById',
    'usp_Release_SetFolderPath',
    'usp_DeleteRelease',
    'usp_ReleaseSignOff_GetByRelease'
)
ORDER BY name;

PRINT '== Phase 2 update completed ==';
GO
