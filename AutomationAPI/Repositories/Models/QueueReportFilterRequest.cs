namespace AutomationAPI.Repositories.Models
{
    public class QueueReportFilterRequest : PagingModel
    {
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
