namespace AutomationAPI.Repositories.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ResultStatus { get; set; }
        public string? Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Message { get; set; }
        public string? ClassName { get; set; }
        public string? QueueId { get; set; }
    }

}
