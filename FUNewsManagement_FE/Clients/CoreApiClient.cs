using System.Text;
using System.Text.Json;

namespace FUNewsManagement_FE.Clients
{
    public interface ICoreApiClient
    {
        Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null);
        Task<T?> SendAndDeserializeAsync<T>(HttpMethod method, string path, object? body = null);
    }

    public class CoreApiClient : ICoreApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public CoreApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
        {
            var req = new HttpRequestMessage(method, path);
            if (body != null && method != HttpMethod.Get)
            {
                req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            }
            return await _http.SendAsync(req);
        }

        public async Task<T?> SendAndDeserializeAsync<T>(HttpMethod method, string path, object? body = null)
        {
            var response = await SendAsync(method, path, body);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }
    }
}
