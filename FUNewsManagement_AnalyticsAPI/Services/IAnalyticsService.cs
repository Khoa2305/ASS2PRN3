using FUNewsManagement_AnalyticsAPI.Dto;

namespace FUNewsManagement_AnalyticsAPI.Services
{
    public interface IAnalyticsService
    {
        /// <summary>
        /// Dashboard summary — total articles, status breakdown, grouping by
        /// category / author / day. Supports optional date / category / author filters.
        /// </summary>
        Task<DashboardDto> GetDashboardAsync(AnalyticsFilter filter, string? bearerToken = null);

        /// <summary>
        /// Trending articles ordered by ViewCount descending.
        /// Supports optional date / category / author filters.
        /// </summary>
        Task<List<TrendingArticleDto>> GetTrendingAsync(AnalyticsFilter filter, int top = 10, string? bearerToken = null);

        /// <summary>
        /// Content-based recommendations for a given article id.
        /// Scored by shared tags, same category, and view count.
        /// </summary>
        Task<RecommendDto> GetRecommendationsAsync(string id, int top = 5, string? bearerToken = null);

        /// <summary>
        /// Export filtered articles + dashboard summary to an Excel workbook (.xlsx).
        /// </summary>
        Task<byte[]> ExportExcelAsync(AnalyticsFilter filter, string? bearerToken = null);
    }
}
