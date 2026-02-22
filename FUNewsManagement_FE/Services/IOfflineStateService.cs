using UI.dto.response;

namespace FUNewsManagement_FE.Services
{
    public interface IOfflineStateService
    {
        bool IsOffline { get; set; }
        DashboardDto? CachedDashboard { get; set; }
    }

    public class OfflineStateService : IOfflineStateService
    {
        public bool IsOffline { get; set; } = false;
        public DashboardDto? CachedDashboard { get; set; }
    }
}
