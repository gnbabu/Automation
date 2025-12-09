

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
        END AS HasScreenshots

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
