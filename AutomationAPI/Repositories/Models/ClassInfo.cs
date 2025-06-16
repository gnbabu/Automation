namespace AutomationAPI.Repositories.Models
{
    public class ClassInfo
    {
        public string ClassName { get; set; }
        public List<LibraryMethodInfo> Methods { get; set; } = new();
    }
}
