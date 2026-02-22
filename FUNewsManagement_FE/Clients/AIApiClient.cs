using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Clients
{
    public interface IAIApiClient
    {
        Task<T?> SendAndDeserializeAsync<T>(HttpMethod method, string path, object? body = null);
    }

    public class AIApiClient : IAIApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public AIApiClient(HttpClient http)
        {
            _http = http;
            // Timeout explicitly shorter for AI calls
            _http.Timeout = TimeSpan.FromSeconds(10);
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
    }
}
