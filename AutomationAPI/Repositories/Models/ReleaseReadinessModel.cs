namespace AutomationAPI.Repositories.Models
{
    public class ReleaseReadinessModel
    {
        public bool FolderExists { get; set; }
        public List<string> DllFiles { get; set; } = new();
        public int UsableDllCount { get; set; }
        public bool IsReady { get; set; }
        public string Message { get; set; }
    }
}
