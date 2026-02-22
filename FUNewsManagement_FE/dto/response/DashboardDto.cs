namespace UI.dto.response
{
    public class DashboardDto
    {
        public int TotalArticles         { get; set; }
        public int PublishedArticles     { get; set; }
        public int DraftArticles         { get; set; }
        public int TotalViewCount        { get; set; }
        public List<CategoryStatDto> ByCategory { get; set; } = new();
        public List<AuthorStatDto>   ByAuthor    { get; set; } = new();
        public List<DailyStatDto>    ByDay       { get; set; } = new();
    }

    public class CategoryStatDto
    {
        public string? CategoryName { get; set; }
        public int Count            { get; set; }
        public int TotalViews       { get; set; }
    }

    public class AuthorStatDto
    {
        public string? AuthorName { get; set; }
        public int Count          { get; set; }
        public int TotalViews     { get; set; }
    }

    public class DailyStatDto
    {
        public string? Date { get; set; }   // yyyy-MM-dd
        public int Count    { get; set; }
    }
}
