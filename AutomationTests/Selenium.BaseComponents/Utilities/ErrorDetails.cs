namespace Selenium.BaseComponents.Utilities
{
    public class ErrorDetails
    {
        public string TestCaseTitle { get; set; }
        public string ActualResult { get; set; }
        public string ExpectedResult { get; set; }
        public bool IsPassed { get; set; }
        public string AdditionalMessage { get; set; }

        public static ErrorDetails ProcessError(string actualMessage, string expectedMessage)
        {
            ErrorDetails _errorDetails = new ErrorDetails();
            _errorDetails.ActualResult = actualMessage;
            _errorDetails.ExpectedResult = expectedMessage;
            _errorDetails.IsPassed = (actualMessage == expectedMessage);
            return _errorDetails;
        }

        public static string ReturnExceptionFileLineNumber(Exception ex)
        {
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
            var stackFrame = trace.GetFrame(trace.FrameCount - 1);
            var lineNumber = stackFrame.GetFileLineNumber();
            var file = stackFrame.GetFileName();
            return "Exception occured due to '" + ex.Message + "' in File: '" + file + "' at Line Num# - '" + lineNumber + "'";
        }
    }
}
