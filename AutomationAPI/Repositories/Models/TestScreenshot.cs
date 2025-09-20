namespace AutomationAPI.Repositories.Models
{
    public class TestScreenshot
    {
        public int ID { get; set; }
        public int TestResultId { get; set; }
        public int QueueId { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Screenshot { get; set; } = string.Empty;
        public DateTime TakenAt { get; set; }
    }
}
