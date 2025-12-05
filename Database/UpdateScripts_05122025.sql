
-- Drop existing procedure
IF EXISTS (SELECT 1 FROM sys.objects 
           WHERE object_id = OBJECT_ID('[aut].[usp_GetTestCasesByAssignmentNameAndUser]')
             AND type = 'P')
BEGIN
    DROP PROCEDURE [aut].[usp_GetTestCasesByAssignmentNameAndUser];
END
GO

-- Create new procedure
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

        -- NEW HasScreenshots flag
        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM aut.TestScreenshots TS 
                WHERE TS.AssignmentTestCaseId = TC.AssignmentTestCaseId
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS HasScreenshots

    FROM aut.AssignedTestCases TC
    INNER JOIN aut.TestCaseAssignment A
        ON TC.AssignmentId = A.AssignmentId
    INNER JOIN aut.[User] U
        ON A.AssignedUser = U.UserID
    WHERE A.AssignmentName = @AssignmentName
      AND A.AssignedUser = @AssignedUser
    ORDER BY TC.Priority, TC.TestCaseId;

END
GO
