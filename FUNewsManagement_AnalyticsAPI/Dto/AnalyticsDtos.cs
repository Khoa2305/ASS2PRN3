namespace FUNewsManagement_AnalyticsAPI.Dto
{
    // ── Mirrors CoreAPI NewsArticle response ─────────────────────────────────
    public class NewsArticleDto
    {
        public string NewsArticleId { get; set; } = string.Empty;
        public string? NewsTitle    { get; set; }
        public string? Headline     { get; set; }
        public string? NewsContent  { get; set; }
        public string? NewsSource   { get; set; }
        public DateTime? CreatedDate  { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? NewsStatus    { get; set; }
        public short? CategoryId   { get; set; }
        public string? CategoryName { get; set; }
        public short? CreatedById   { get; set; }
        public string? CreatedByName { get; set; }
        public int ViewCount        { get; set; }
        public List<int> Tags       { get; set; } = new();
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
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

    // ── Trending ─────────────────────────────────────────────────────────────
    public class TrendingArticleDto
    {
        public string NewsArticleId { get; set; } = string.Empty;
        public string? NewsTitle     { get; set; }
        public string? CategoryName  { get; set; }
        public string? AuthorName    { get; set; }
        public int ViewCount         { get; set; }
    }

    // ── Recommend ────────────────────────────────────────────────────────────
    public class RecommendDto
    {
        public string TargetArticleId    { get; set; } = string.Empty;
        public string? TargetTitle       { get; set; }
        public List<RecommendItemDto> Recommendations { get; set; } = new();
    }

    public class RecommendItemDto
    {
        public string NewsArticleId { get; set; } = string.Empty;
        public string? NewsTitle     { get; set; }
        public string? CategoryName  { get; set; }
        public int SharedTags        { get; set; }
        public int ViewCount         { get; set; }
        public double Score          { get; set; }
    }

    // ── Filters ──────────────────────────────────────────────────────────────
    public class AnalyticsFilter
    {
        public DateTime? From       { get; set; }
        public DateTime? To         { get; set; }
        public string?   Category   { get; set; }
        public string?   Author     { get; set; }
    }

    // ── Generic API response (mirrors CoreAPI) ────────────────────────────────
    public class ApiResponse<T>
    {
        public bool    Success { get; set; }
        public string? Message { get; set; }
        public T?      Data    { get; set; }
    }
}
