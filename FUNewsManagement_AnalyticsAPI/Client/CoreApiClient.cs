using FUNewsManagement_AnalyticsAPI.Dto;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FUNewsManagement_AnalyticsAPI.Client
{
    public class CoreApiClient : ICoreApiClient
    {
        private readonly HttpClient _http;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CoreApiClient(HttpClient http)
        {
            _http = http;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static void ApplyBearer(HttpClient http, string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<List<T>> GetListAsync<T>(string url, string? token)
        {
            ApplyBearer(_http, token);
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<T>();

            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<List<T>>>(json, _jsonOpts);
            return wrapper?.Data ?? new List<T>();
        }

        private async Task<T?> GetSingleAsync<T>(string url, string? token)
        {
            ApplyBearer(_http, token);
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOpts);
            return wrapper != null ? wrapper.Data : default;
        }

        // ── interface implementation ─────────────────────────────────────────

        public Task<List<NewsArticleDto>> GetAllArticlesAsync(string? bearerToken = null)
            => GetListAsync<NewsArticleDto>("api/newsarticles", bearerToken);

        public Task<List<NewsArticleDto>> GetTrendingArticlesAsync(int top = 10, string? bearerToken = null)
            => GetListAsync<NewsArticleDto>($"api/newsarticles/trending?top={top}", bearerToken);

        public Task<NewsArticleDto?> GetArticleByIdAsync(string id, string? bearerToken = null)
            => GetSingleAsync<NewsArticleDto>($"api/newsarticles/{id}", bearerToken);
    }
}
