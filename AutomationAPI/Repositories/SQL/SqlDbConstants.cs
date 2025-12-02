namespace AutomationAPI.Repositories.SQL
{
    public static class SqlDbConstants
    {
        //User and Usr Role Stored Procedures
        public const string GetAllUsers = "[aut].[usp_GetAllUsers]";
        public const string GetUserById = "[aut].[usp_GetUserById]";
        public const string ValidateUserByUsernameAndPassword = "[aut].[usp_ValidateUserByUsernameAndPassword]";
        public const string CreateUser = "[aut].[usp_CreateUser]";
        public const string UpdateUser = "[aut].[usp_UpdateUser]";
        public const string DeleteUser = "[aut].[usp_DeleteUser]";
        public const string ChangePassword = "[aut].[usp_ChangePassword]";
        public const string SetUserActiveStatus = "[aut].[usp_SetUserActiveStatus]";
        public const string GetUserRoles = "[aut].[usp_GetUserRoles]";
        public const string FilterAllUsers = "[aut].[usp_GetFilteredUsers]";
        public const string RegisterUser = "[aut].[usp_RegisterUser]";
        public const string GetUserStatuses = "[aut].[usp_GetUserStatuses]";
        public const string GetTimeZones = "[aut].[usp_GetTimeZones]";
        public const string GetPriorityStatus = "[aut].[usp_GetPriorityStatus]";


        //Automation Data
        public const string GetAutomationData = "[aut].[usp_GetAutomationData]";
        public const string GetAutomationDataByFlowName = "[aut].[usp_GetAutomationDataByFlowName]";
        public const string InsertAutomationData = "[aut].[usp_InsertAutomationData]";
        public const string UpdateAutomationData = "[aut].[usp_UpdateAutomationData]";
        public const string DeleteAutomationData = "[aut].[usp_DeleteAutomationData]";

        public const string GetAutomationDataSection = "[aut].[usp_get_AutomationDataSection]";
        public const string GetAutomationFlowNames = "[aut].[usp_get_AutomationFlowNames]";
        public const string InsertAutomationDataSection = "[aut].[usp_InsertAutomationDataSection]";
        public const string UpdateAutomationDataSections = "[aut].[usp_UpdateAutomationDataSections]";
        public const string DeleteAutomationDataSection = "[aut].[usp_DeleteAutomationDataSection]";


        // Test Sceenshots
        public const string InsertTestScreenshot = "[aut].[usp_InsertTestScreenshot]";
        public const string BulkInsertTestScreenshots = "[aut].[usp_BulkInsertTestScreenshots]";
        public const string usp_GetScreenshotsByQueueId = "[aut].[usp_GetScreenshotsByQueueId]";
        public const string GetScreenshotById = "[aut].[usp_GetScreenshotById]";


        //Test Case Assignments
        public const string CreateOrUpdateAssignmentWithTestCases = "[aut].[usp_CreateOrUpdateAssignmentWithTestCases]";
        public const string GetTestCasesByAssignmentNameAndUser = "[aut].[usp_GetTestCasesByAssignmentNameAndUser]";
        public const string GetAssignedTestCasesForLibraryAndEnvironment = "[aut].[usp_GetAssignedTestCasesForLibraryAndEnvironment]";
        public const string GetAllAssignedTestCasesInLibrary = "[aut].[usp_GetAllAssignedTestCasesInLibrary]";
        public const string GetTestCaseAssignmentsByUser = "[aut].[usp_GetTestCaseAssignmentsByUser]";

        public const string UpdateAssignedTestCaseStatus = "[aut].[usp_UpdateAssignedTestCaseStatus]";

        //New Queue Implementation

        public const string AssignmentTestCaseIdList = "[aut].[AssignmentTestCaseIdList]";
        public const string SingleRunTestCaseNow = "[aut].[usp_SingleRunTestCaseNow]";
        public const string BulkRunTestCasesNow = "[aut].[usp_BulkRunTestCasesNow]";
        public const string ScheduleSingleTestCase = "[aut].[usp_ScheduleSingleTestCase]";
        public const string BulkScheduleTestCases = "[aut].[usp_BulkScheduleTestCases]";
        public const string GetPendingExecutionQueues = "[aut].[usp_GetPendingExecutionQueues]";
        public const string UpdateQueueStatus = "[aut].[usp_UpdateQueueStatus]";

    }
}
