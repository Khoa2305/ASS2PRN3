using System.Text.Json;
using FUNewsManagement_FE.Clients;
using FUNewsManagement_FE.Services;
using UI.dto.response;

namespace FUNewsManagement_FE.Workers
{
    public class DashboardSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOfflineStateService _offlineState;
        private readonly ILogger<DashboardSyncWorker> _logger;
        private readonly IWebHostEnvironment _env;

        private const string CacheFileName = "dashboard_cache.json";

        public DashboardSyncWorker(
            IServiceProvider serviceProvider, 
            IOfflineStateService offlineState, 
            ILogger<DashboardSyncWorker> logger,
            IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _offlineState = offlineState;
            _logger = logger;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial load from disk on startup if memory is empty
            await LoadCacheFromDiskAsync();

            // Setup a timer that ticks every 6 hours
            using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

            // Run immediately on first thread allocation, then wait for timer
            do
            {
                if (stoppingToken.IsCancellationRequested) break;

                await SyncDashboardAsync();

            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task SyncDashboardAsync()
        {
            try
            {
                _logger.LogInformation("DashboardSyncWorker checking for fresh analytics data...");

                // Create a scope to resolve scoped/transient services like typed HttpClients
                using var scope = _serviceProvider.CreateScope();
                var analyticsApi = scope.ServiceProvider.GetRequiredService<IAnalyticsApiClient>();

                // Assuming the Analytics API has a public GET /api/analytics/dashboard endpoint 
                // Alternatively this might require an admin token. We will try to fetch it.
                var response = await analyticsApi.SendAndDeserializeAsync<ApiResponse<DashboardDto>>(
                    HttpMethod.Get, 
                    "api/analytics/dashboard");

                if (response != null && response.Success && response.Data != null)
                {
                    _logger.LogInformation("Successfully fetched analytics dashboard. Updating cache.");
                    
                    // 1. Update Memory
                    _offlineState.CachedDashboard = response.Data;
                    _offlineState.IsOffline = false;

                    // 2. Save to Disk
                    await SaveCacheToDiskAsync(response.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch analytics dashboard (Unsuccessful API response). Going offline.");
                    _offlineState.IsOffline = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching analytics dashboard. Going offline.");
                _offlineState.IsOffline = true;
            }
        }

        private async Task LoadCacheFromDiskAsync()
        {
            if (_offlineState.CachedDashboard != null) return;

            var cachePath = Path.Combine(_env.WebRootPath, "data", CacheFileName);
            if (!File.Exists(cachePath)) return;

            try
            {
                var json = await File.ReadAllTextAsync(cachePath);
                var data = JsonSerializer.Deserialize<DashboardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null)
                {
                    _offlineState.CachedDashboard = data;
                    _logger.LogInformation("Loaded old dashboard data from disk cache.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard cache from disk.");
            }
        }

        private async Task SaveCacheToDiskAsync(DashboardDto data)
        {
            try
            {
                var dataDir = Path.Combine(_env.WebRootPath, "data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                
                var cachePath = Path.Combine(dataDir, CacheFileName);
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(cachePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save dashboard cache to disk.");
            }
        }
    }
}
