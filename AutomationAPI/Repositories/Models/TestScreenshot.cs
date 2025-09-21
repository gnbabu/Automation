namespace AutomationAPI.Repositories.Models
{
    public class TestScreenshot
    {
        public int ID { get; set; }
        public string QueueId { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public string Caption { get; set; } = string.Empty;
        public string Screenshot { get; set; } = string.Empty;
        public DateTime TakenAt { get; set; }
    }

}
