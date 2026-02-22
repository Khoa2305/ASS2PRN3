using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Clients
{
    public interface IAnalyticsApiClient
    {
        Task<T?> SendAndDeserializeAsync<T>(HttpMethod method, string path, object? body = null);
        Task<byte[]> DownloadExcelAsync(string path);
    }

    public class AnalyticsApiClient : IAnalyticsApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public AnalyticsApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<T?> SendAndDeserializeAsync<T>(HttpMethod method, string path, object? body = null)
        {
            var req = new HttpRequestMessage(method, path);
            if (body != null && method != HttpMethod.Get)
            {
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            }
            var response = await _http.SendAsync(req);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        public async Task<byte[]> DownloadExcelAsync(string path)
        {
            var response = await _http.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
