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

                using var scope = _serviceProvider.CreateScope();
                var analyticsApi = scope.ServiceProvider.GetRequiredService<IAnalyticsApiClient>();

                var response = await analyticsApi.SendAndDeserializeAsync<ApiResponse<DashboardDto>>(
                    HttpMethod.Get,
                    "api/analytics/dashboard");

                if (response != null && response.Success && response.Data != null)
                {
                    _logger.LogInformation("Successfully fetched analytics dashboard.");

                    _offlineState.CachedDashboard = response.Data;
                    _offlineState.IsOffline = false;

                    await SaveCacheToDiskAsync(response.Data);
                }
                else
                {
                    // API trả về nhưng không thành công
                    // KHÔNG đánh dấu offline
                    _logger.LogWarning("API responded but unsuccessful. Not marking offline.");
                }
            }
            catch (HttpRequestException ex)
            {
                // Chỉ network error mới đánh dấu offline
                _logger.LogError(ex, "Network error. Backend may be down.");
                _offlineState.IsOffline = true;
            }
            catch (TaskCanceledException ex)
            {
                // Timeout cũng tính là network issue
                _logger.LogError(ex, "Request timeout. Going offline.");
                _offlineState.IsOffline = true;
            }
            catch (Exception ex)
            {
                // Lỗi logic khác → không phải backend chết
                _logger.LogWarning(ex, "Unexpected error but not marking offline.");
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
