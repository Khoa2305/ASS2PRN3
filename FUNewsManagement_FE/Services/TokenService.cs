using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text;

namespace FUNewsManagement_FE.Services
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        // Fresh client to avoid DI circular loops with IHttpClientFactory when refreshing tokens
        private static readonly HttpClient _refreshClient = new HttpClient();

        public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("ACCESS_TOKEN");
        }

        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("REFRESH_TOKEN");
        }

        public bool IsTokenExpired(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return true;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken == null) return true;

                // Check if token expires within the next 30 seconds
                return jwtToken.ValidTo <= DateTime.UtcNow.AddSeconds(30);
            }
            catch
            {
                // Can't parse token, consider it expired
                return true;
            }
        }

        public async Task<string?> RefreshTokenAsync()
        {
            var refreshToken = GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return null;

            var baseUrl = _configuration["ApiSettings:Assignment1"] ?? "http://localhost:5039/";
            baseUrl = baseUrl.TrimEnd('/');
            var url = $"{baseUrl}/api/auth/refresh";

            var payload = new { RefreshToken = refreshToken };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _refreshClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiWrapper = JsonSerializer.Deserialize<ApiResponseWrapper>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (apiWrapper?.Data != null && !string.IsNullOrEmpty(apiWrapper.Data.AccessToken))
                    {
                        var session = _httpContextAccessor.HttpContext?.Session;
                        if (session != null)
                        {
                            session.SetString("ACCESS_TOKEN", apiWrapper.Data.AccessToken);
                            if (!string.IsNullOrEmpty(apiWrapper.Data.RefreshToken))
                            {
                                session.SetString("REFRESH_TOKEN", apiWrapper.Data.RefreshToken);
                            }
                        }
                        return apiWrapper.Data.AccessToken;
                    }
                }
            }
            catch
            {
                // Ignore network exceptions, return null to fail refresh
            }

            return null;
        }

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
