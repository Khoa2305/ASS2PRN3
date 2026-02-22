using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Assignment1.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IAuthenticationService _authService)
        {
            // 1. Check for [AllowAnonymous] metadata
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // 2. Fallback for paths that might not have endpoint metadata (Swagger, static files, etc.)
            var excludedPaths = new[]
            {
                "/api/auth/login",
                "/api/newsarticles/public",
                "/swagger",
                "/favicon.ico"
            };

            if (excludedPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }
            string accessToken;
            try
            {
                accessToken = context.Request.Headers["Authorization"].ToString()?.Substring(7);
            }
            catch
            {
                accessToken = null;
            }
            if (!string.IsNullOrEmpty(accessToken) && _authService.IsValidAccessToken(accessToken, out ClaimsPrincipal claims))
            {
                context.User = claims;
                await _next(context);
                return;
            }
            else
            {
                throw new AppException(Fail.EXPIRE_TOKEN);
            }
        }
    }
}
