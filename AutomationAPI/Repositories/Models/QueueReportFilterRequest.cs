namespace AutomationAPI.Repositories.Models
{
    public class QueueReportFilterRequest : PagingModel
    {
        public int UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
