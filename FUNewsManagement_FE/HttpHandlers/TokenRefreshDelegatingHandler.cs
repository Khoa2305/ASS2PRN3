using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace FUNewsManagement_FE.HttpHandlers
{
    public class TokenRefreshDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        // Note: Using a fresh HttpClient to avoid circular dependency loops with IHttpClientFactory
        private static readonly HttpClient _refreshClient = new HttpClient();

        public TokenRefreshDelegatingHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = session.GetString("ACCESS_TOKEN");
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // If 401 Unauthorized, try to refresh the token automatically
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                var refreshToken = session.GetString("REFRESH_TOKEN");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var newTokens = await AttemptTokenRefreshAsync(refreshToken);

                    if (newTokens != null && !string.IsNullOrEmpty(newTokens.AccessToken))
                    {
                        // Update session with new tokens
                        session.SetString("ACCESS_TOKEN", newTokens.AccessToken);
                        if (!string.IsNullOrEmpty(newTokens.RefreshToken))
                        {
                            session.SetString("REFRESH_TOKEN", newTokens.RefreshToken);
                        }

                        // Retry the original request with the *new* access token
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);
                        
                        // We must clone the request content because it might have been consumed on the first try
                        var clonedRequest = await CloneRequestAsync(request);
                        
                        // Dispose the old 401 response and send again
                        response.Dispose();
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                    }
                }
            }

            return response;
        }

        private async Task<TokenRefreshResponse?> AttemptTokenRefreshAsync(string refreshToken)
        {
            // CoreAPI is likely at https://localhost:7001, wait, let's use the UI's existing BaseUrl (5039 or appsettings)
            // UI AuthController used http://localhost:5039 previously. Let's pull from config if available, fallback to 5039.
            var baseUrl = _configuration["CoreApi:BaseUrl"] ?? "http://localhost:5039";
            var url = $"{baseUrl}/api/auth/refresh";

            var payload = new { RefreshToken = refreshToken };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var refreshResponse = await _refreshClient.PostAsync(url, content);
                if (refreshResponse.IsSuccessStatusCode)
                {
                    var json = await refreshResponse.Content.ReadAsStringAsync();
                    var apiWrapper = JsonSerializer.Deserialize<ApiResponseWrapper>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return apiWrapper?.Data;
                }
            }
            catch
            {
                // Ignoring exceptions during refresh — will just fall through to returning 401.
            }

            return null;
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            if (req.Content != null)
            {
                var bytes = await req.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(bytes);
                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                    {
                        clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
                    }
                }
            }
            
            clone.Version = req.Version;

            foreach (var prop in req.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
            }

            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        // Inner classes to map the JSON structure from /api/auth/refresh
        private class ApiResponseWrapper
        {
            public bool Success { get; set; }
            public TokenRefreshResponse? Data { get; set; }
        }

        private class TokenRefreshResponse
        {
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
