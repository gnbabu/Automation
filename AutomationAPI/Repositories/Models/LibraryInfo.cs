namespace AutomationAPI.Repositories.Models
{
    public class LibraryInfo
    {
        public string LibraryName { get; set; }
        public List<ClassInfo> Classes { get; set; } = new();
    }
}
