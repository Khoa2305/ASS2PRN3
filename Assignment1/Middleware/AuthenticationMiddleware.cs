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
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

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

            string? accessToken = null;

            // 🔥 1. Try header first
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader.Substring("Bearer ".Length);
            }

            // 🔥 2. If not found → try SignalR query
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = context.Request.Query["access_token"];
            }

            if (!string.IsNullOrEmpty(accessToken) &&
                _authService.IsValidAccessToken(accessToken, out ClaimsPrincipal claims))
            {
                context.User = claims;
                await _next(context);
                return;
            }

            throw new AppException(Fail.EXPIRE_TOKEN);
        }
    }
}
