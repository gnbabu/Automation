namespace AutomationAPI.Repositories.Models
{
    // Distinguishes real business-rule violations from genuine server errors so
    // AutomationController can return a clean 409 with the actual reason instead of a
    // generic 500 - see AGENTS.md "Flow & Section management" for why these checks
    // exist (usp_InsertAutomationDataSection had no uniqueness check at all, and
    // usp_DeleteAutomationDataSection had no protection against orphaning existing
    // AutomationData - confirmed via sys.foreign_keys there is no FK constraint for
    // this relationship at all).
    public class DuplicateSectionException : Exception
    {
        public DuplicateSectionException(string message) : base(message) { }
    }

    public class SectionHasDataException : Exception
    {
        public int DataCount { get; }

        public SectionHasDataException(string message, int dataCount) : base(message)
        {
            DataCount = dataCount;
        }
    }
}
