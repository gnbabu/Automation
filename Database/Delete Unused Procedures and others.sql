-- ============================================
-- Drop Procedures in [aut] schema
-- ============================================
IF OBJECT_ID('[aut].[usp_DeleteTestCaseAssignments]', 'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_DeleteTestCaseAssignments];
GO

IF OBJECT_ID('[aut].[usp_SyncTestCaseAssignments_New]', 'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_SyncTestCaseAssignments_New];
GO

IF OBJECT_ID('[aut].[usp_DeleteTestCaseAssignmentsByUser]', 'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_DeleteTestCaseAssignmentsByUser];
GO

IF OBJECT_ID('[aut].[usp_SyncTestCaseAssignments]', 'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_SyncTestCaseAssignments];
GO

IF OBJECT_ID('[aut].[usp_InsertTestCaseAssignment]', 'P') IS NOT NULL
    DROP PROCEDURE [aut].[usp_InsertTestCaseAssignment];
GO
IF OBJECT_ID('[aut].[usp_ValidateUserByEmailAndPassword]', 'P') IS NOT NULL
	DROP PROCEDURE [aut].[usp_ValidateUserByEmailAndPassword]

-- ============================================
-- Drop Table Type in [aut] schema
-- ============================================
IF TYPE_ID(N'[aut].[TestCaseAssignmentType]') IS NOT NULL
    DROP TYPE [aut].[TestCaseAssignmentType];
GO