namespace AutomationAPI.Repositories.SQL
{
    public static class SqlDbConstants
    {
        //User and Usr Role Stored Procedures
        public const string GetAllUsers = "[aut].[usp_GetAllUsers]";
        public const string GetUserById = "[aut].[usp_GetUserById]";
        public const string ValidateUserByEmailAndPassword = "[aut].[usp_ValidateUserByEmailAndPassword]";
        public const string CreateUser = "[aut].[usp_CreateUser]";
        public const string UpdateUser = "[aut].[usp_UpdateUser]";
        public const string DeleteUser = "[aut].[usp_DeleteUser]";
        public const string ChangePassword = "[aut].[usp_ChangePassword]";
        public const string SetUserActiveStatus = "[aut].[usp_SetUserActiveStatus]";
        public const string GetUserRoles = "[aut].[usp_GetUserRoles]";

        //Queue Info
        public const string GetAllQueues = "[aut].[usp_GetAllQueues]";
        public const string GetQueueById = "[aut].[usp_GetQueueById]";
        public const string GetQueues = "[aut].[usp_GetQueues]";
        public const string InsertQueue = "[aut].[usp_InsertQueue]";
        public const string UpdateQueue = "[aut].[usp_UpdateQueue]";
        public const string DeleteQueue = "[aut].[usp_DeleteQueue]";
        public const string GetQueueReports = "[aut].[usp_GetQueueReports]";
        public const string SearchQueues = "[aut].[usp_SearchQueues]";


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


        //Test Results

        public const string SearchTestResults = "[aut].[usp_SearchTestResults]";
        public const string GetAllTestResults = "[aut].[usp_GetAllTestResults]";
        public const string GetTestResults = "[aut].[usp_GetTestResults]";
        public const string InsertTestResults = "[aut].[usp_InsertTestResults]";

        // Test Sceenshots
        public const string InsertTestScreenshot = "[aut].[usp_InsertTestScreenshot]";
        public const string BulkInsertTestScreenshots = "[aut].[usp_BulkInsertTestScreenshots]";
        public const string usp_GetScreenshotsByQueueId = "[aut].[usp_GetScreenshotsByQueueId]";
        public const string GetScreenshotById = "[aut].[usp_GetScreenshotById]";

        
        public const string GetTestCaseAssignmentsByUser = "[aut].[usp_GetTestCaseAssignmentsByUser]";
        public const string InsertTestCaseAssignment = "[aut].[usp_InsertTestCaseAssignment]";
        public const string TestCaseAssignmentType = "[aut].[TestCaseAssignmentType]";
        public const string BulkSyncTestCaseAssignments = "[aut].[usp_SyncTestCaseAssignments]";
        public const string DeleteTestCaseAssignmentsByUser = "[aut].[usp_DeleteTestCaseAssignmentsByUser]";
        
    }
}
