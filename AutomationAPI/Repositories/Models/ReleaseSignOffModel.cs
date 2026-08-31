namespace AutomationAPI.Repositories.Models
{
    public class ReleaseSignOffModel
    {
        public int ReleaseSignOffId { get; set; }
        public int ReleaseId { get; set; }
        public string SignOffStatus { get; set; }
        public string SignOffBy { get; set; }
        public DateTime? SignOffOn { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
