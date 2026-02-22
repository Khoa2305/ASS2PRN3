namespace FUNewsManagement_FE.dto.response
{
    public class TrendingArticleDto
    {
        public string NewsArticleId { get; set; } = string.Empty;
        public string? NewsTitle     { get; set; }
        public string? CategoryName  { get; set; }
        public string? AuthorName    { get; set; }
        public int ViewCount         { get; set; }
    }
}
