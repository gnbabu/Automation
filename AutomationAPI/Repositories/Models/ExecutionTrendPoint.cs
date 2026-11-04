namespace AutomationAPI.Repositories.Models
{
    public class ExecutionTrendPoint
    {
        public DateTime Date { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Skipped { get; set; }
        public int Running { get; set; }
    }
}
