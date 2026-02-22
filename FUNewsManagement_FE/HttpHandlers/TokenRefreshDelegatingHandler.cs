using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

using FUNewsManagement_FE.Services;

namespace FUNewsManagement_FE.HttpHandlers
{
    public class TokenRefreshDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public TokenRefreshDelegatingHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = _tokenService.GetAccessToken();

            // 1. Proactive Expiration Check
            if (!string.IsNullOrEmpty(accessToken) && _tokenService.IsTokenExpired(accessToken))
            {
                // Token is strictly expired or about to expire, attempt refresh before sending request
                accessToken = await _tokenService.RefreshTokenAsync() ?? accessToken;
            }

            // 2. Attach Token
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // 3. Reactive Refresh (Safety Net for 401s)
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                var newAccessToken = await _tokenService.RefreshTokenAsync();
                
                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                    
                    var clonedRequest = await CloneRequestAsync(request);
                    response.Dispose();
                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
            }

            return response;
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
    }
}
