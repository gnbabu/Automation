namespace AutomationAPI.Repositories.Models
{
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
