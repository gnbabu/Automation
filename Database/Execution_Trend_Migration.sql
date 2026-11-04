SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('aut.TestCaseExecutionQueue', 'TestCaseStatus') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD TestCaseStatus NVARCHAR(50) NULL;
IF COL_LENGTH('aut.TestCaseExecutionQueue', 'StartTime') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD StartTime DATETIME NULL;
IF COL_LENGTH('aut.TestCaseExecutionQueue', 'EndTime') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD EndTime DATETIME NULL;
IF COL_LENGTH('aut.TestCaseExecutionQueue', 'DurationSeconds') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD DurationSeconds FLOAT NULL;
IF COL_LENGTH('aut.TestCaseExecutionQueue', 'ErrorMessage') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD ErrorMessage NVARCHAR(MAX) NULL;
IF COL_LENGTH('aut.TestCaseExecutionQueue', 'ResultRecordedOn') IS NULL
    ALTER TABLE aut.TestCaseExecutionQueue ADD ResultRecordedOn DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TestCaseExecutionQueue_Result' AND object_id = OBJECT_ID('aut.TestCaseExecutionQueue'))
    CREATE INDEX IX_TestCaseExecutionQueue_Result
        ON aut.TestCaseExecutionQueue (AssignmentTestCaseId, ResultRecordedOn)
        INCLUDE (TestCaseStatus);
GO

CREATE OR ALTER PROCEDURE aut.usp_UpdateQueueResult
(
    @QueueId        UNIQUEIDENTIFIER,
    @TestCaseStatus NVARCHAR(50),
    @StartTime      DATETIME = NULL,
    @EndTime        DATETIME = NULL,
    @Duration       FLOAT = NULL,
    @ErrorMessage   NVARCHAR(MAX) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF (@StartTime IS NOT NULL AND @EndTime IS NOT NULL AND (@Duration IS NULL OR @Duration <= 0))
        SET @Duration = DATEDIFF(SECOND, @StartTime, @EndTime);

    UPDATE aut.TestCaseExecutionQueue
    SET TestCaseStatus   = @TestCaseStatus,
        StartTime        = @StartTime,
        EndTime          = @EndTime,
        DurationSeconds  = @Duration,
        ErrorMessage     = @ErrorMessage,
        ResultRecordedOn = GETDATE()
    WHERE QueueId = @QueueId;

    SELECT @@ROWCOUNT AS UpdatedRows;
END
GO

;WITH LatestRun AS
(
    SELECT q.Id, q.AssignmentTestCaseId,
           ROW_NUMBER() OVER (PARTITION BY q.AssignmentTestCaseId ORDER BY q.Id DESC) AS rn
    FROM aut.TestCaseExecutionQueue q
    WHERE q.QueueStatus IN ('Completed', 'Failed')
)
UPDATE q
SET TestCaseStatus   = atc.TestCaseStatus,
    StartTime        = atc.StartTime,
    EndTime          = atc.EndTime,
    DurationSeconds  = atc.Duration,
    ErrorMessage     = atc.ErrorMessage,
    ResultRecordedOn = COALESCE(q.ModifiedDate, q.ExecutionDateTime, q.CreatedDate)
FROM aut.TestCaseExecutionQueue q
JOIN LatestRun lr ON lr.Id = q.Id AND lr.rn = 1
JOIN aut.AssignedTestCases atc ON atc.AssignmentTestCaseId = q.AssignmentTestCaseId
WHERE q.TestCaseStatus IS NULL
  AND atc.TestCaseStatus IN ('Passed', 'Failed', 'Skipped', 'Cancelled', 'Inconclusive');
GO

CREATE OR ALTER PROCEDURE aut.usp_GetReleaseExecutionTrend
(
    @ReleaseId    INT,
    @Days         INT = 7,
    @AssignedUser INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Days IS NULL OR @Days < 1 SET @Days = 7;
    IF @Days > 90 SET @Days = 90;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    ;WITH Numbers AS
    (
        SELECT TOP (@Days) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
        FROM sys.all_objects
    ),
    DayList AS
    (
        SELECT DATEADD(DAY, -n, @Today) AS TrendDate FROM Numbers
    ),
    ReleaseCases AS
    (
        SELECT atc.AssignmentTestCaseId, atc.TestCaseStatus AS CurrentStatus
        FROM aut.AssignedTestCases atc
        JOIN aut.TestCaseAssignment tca ON tca.AssignmentId = atc.AssignmentId
        WHERE tca.ReleaseId = @ReleaseId
          AND (@AssignedUser IS NULL OR tca.AssignedUser = @AssignedUser)
    ),
    StatusPerDay AS
    (
        SELECT d.TrendDate, rc.AssignmentTestCaseId, lastResult.TestCaseStatus
        FROM DayList d
        CROSS JOIN ReleaseCases rc
        OUTER APPLY
        (
            SELECT TOP (1) q.TestCaseStatus
            FROM aut.TestCaseExecutionQueue q
            WHERE q.AssignmentTestCaseId = rc.AssignmentTestCaseId
              AND q.TestCaseStatus IS NOT NULL
              AND q.ResultRecordedOn < DATEADD(DAY, 1, CAST(d.TrendDate AS DATETIME))
            ORDER BY q.ResultRecordedOn DESC, q.Id DESC
        ) lastResult
    )
    SELECT
        d.TrendDate,
        SUM(CASE WHEN s.TestCaseStatus = 'Passed' THEN 1 ELSE 0 END) AS Passed,
        SUM(CASE WHEN s.TestCaseStatus = 'Failed' THEN 1 ELSE 0 END) AS Failed,
        SUM(CASE WHEN s.TestCaseStatus IN ('Skipped', 'Cancelled', 'Inconclusive') THEN 1 ELSE 0 END) AS Skipped,
        CASE WHEN d.TrendDate = @Today
             THEN (SELECT COUNT(*) FROM ReleaseCases rc WHERE rc.CurrentStatus IN ('InProgress', 'Queued', 'Scheduled'))
             ELSE 0 END AS Running
    FROM DayList d
    LEFT JOIN StatusPerDay s ON s.TrendDate = d.TrendDate
    GROUP BY d.TrendDate
    ORDER BY d.TrendDate;
END
GO
