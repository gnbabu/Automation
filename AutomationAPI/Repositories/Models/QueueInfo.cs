namespace AutomationAPI.Repositories.Models
{
    using System;

    public class QueueInfo
    {
        public string? QueueId { get; set; }
        public string? TagName { get; set; }
        public string? EmpId { get; set; }
        public string? QueueName { get; set; }
        public string? QueueDescription { get; set; }
        public string? ProductLine { get; set; }
        public string? QueueStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int Id { get; set; }
        public string? LibraryName { get; set; }
        public string? ClassName { get; set; }
        public string? MethodName { get; set; }
        public int UserId { get; set; }
    }

}
