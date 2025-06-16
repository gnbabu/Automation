namespace AutomationAPI.Repositories.Models
{
    public class PagingModel
    {
        public int Page { get; set; } = 1;// Current page number (1-based)
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string SortDirection { get; set; } = "ASC"; // "ASC" or "DESC"
    }
}
