namespace AutomationAPI.Repositories.Models
{
    public class ReleaseNotifyResult
    {
        public int Recipients { get; set; }
        public int Sent { get; set; }
        public int Failed { get; set; }
    }
}
