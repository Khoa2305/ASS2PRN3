using FUNewsManagement_AnalyticsAPI.Dto;

namespace FUNewsManagement_AnalyticsAPI.Client
{
    public interface ICoreApiClient
    {
        /// <summary>Fetch all news articles from CoreAPI (GET /api/newsarticles).</summary>
        Task<List<NewsArticleDto>> GetAllArticlesAsync(string? bearerToken = null);

        /// <summary>Fetch trending articles (GET /api/newsarticles/trending?top=N).</summary>
        Task<List<NewsArticleDto>> GetTrendingArticlesAsync(int top = 10, string? bearerToken = null);

        /// <summary>Fetch a single article by id (GET /api/newsarticles/{id}).</summary>
        Task<NewsArticleDto?> GetArticleByIdAsync(string id, string? bearerToken = null);
    }
}
