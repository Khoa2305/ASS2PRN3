using FUNewsManagement_FE.dto.response;
using UI.dto.response;

namespace FUNewsManagement_FE.Models
{
    public class AnalyticsViewModel
    {
        public DashboardDto DashboardMetrics { get; set; } = new DashboardDto();
        public List<TrendingArticleDto> TrendingArticles { get; set; } = new();
    }
}
