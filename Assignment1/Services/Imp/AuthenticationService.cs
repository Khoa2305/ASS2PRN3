using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Assignment1.Services.Imp
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IAccountRepository accountRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        // ── Existing — untouched ─────────────────────────────────────────────

        public SystemAccount isCorrectAccount(string username, string password)
        {
            string adminEmail    = _configuration["AdminAccount:Email"]!;
            string adminPassword = _configuration["AdminAccount:Password"]!;

            if (username.Equals(adminEmail) && password.Equals(adminPassword))
            {
                return new SystemAccount
                {
                    AccountEmail    = adminEmail,
                    AccountRole     = null, // Admin role is null to distinguish from Staff (1) and Lecturer (2)
                    AccountId       = 999,
                    AccountName     = "Admin",
                    AccountPassword = password,
                };
            }

            var accounts = _accountRepository.GetAccountsAsync().GetAwaiter().GetResult();
            SystemAccount? account = accounts.FirstOrDefault(s => s.AccountEmail == username && s.AccountPassword == password);

            if (account == null)
                throw new AppException(Fail.WRONG_ACCOUNT);

            return account;
        }

        public string GenerateAccessToken(SystemAccount account)
        {
            string key    = _configuration["Jwt:Key"]!;
            string issuer = _configuration["Jwt:Issuer"]!;
            string aud    = _configuration["Jwt:Audience"]!;
            int    expMin = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "30");

            var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds   = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256);

            // Role mapping: null = Admin, 1 = Staff, 2 = Lecturer
            string role = "Admin";
            if (account.AccountRole == 1) role = "1";
            else if (account.AccountRole == 2) role = "2";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.AccountEmail ?? ""),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           aud,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expMin),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool IsValidAccessToken(string accessToken, out ClaimsPrincipal claimsPrincipal)
        {
            string key    = _configuration["Jwt:Key"]!;
            string issuer = _configuration["Jwt:Issuer"]!;
            string aud    = _configuration["Jwt:Audience"]!;

            var handler    = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidIssuer              = issuer,
                ValidateAudience         = true,
                ValidAudience            = aud,
                ValidateLifetime         = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ClockSkew                = TimeSpan.Zero,
            };
            try
            {
                claimsPrincipal = handler.ValidateToken(accessToken, parameters, out _);
                return true;
            }
            catch
            {
                claimsPrincipal = null!;
                return false;
            }
        }

        // ── New — Refresh Token ──────────────────────────────────────────────

        public async Task<string> GenerateRefreshTokenAsync(SystemAccount account)
        {
            int expDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

            var refreshToken = new RefreshToken
            {
                Id             = Guid.NewGuid(),
                AccountId      = account.AccountId,
                Token          = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"), // 64-char opaque token
                ExpirationDate = DateTime.UtcNow.AddDays(expDays),
                IsRevoked      = false,
                CreatedDate    = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            return refreshToken.Token;
        }

        public async Task<(string AccessToken, string NewRefreshToken)> RefreshAsync(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (stored == null || stored.IsRevoked || stored.ExpirationDate <= DateTime.UtcNow)
                throw new AppException(Fail.EXPIRE_TOKEN);

            // Revoke old token (rotation)
            await _refreshTokenRepository.RevokeAsync(stored);

            // Reconstruct a minimal SystemAccount from the stored token to sign a new access token
            var accounts       = await _accountRepository.GetAccountsAsync();
            SystemAccount? account = accounts.FirstOrDefault(a => a.AccountId == stored.AccountId);

            // Special case: Admin (AccountId = 999) does not exist in DB
            if (account == null)
            {
                if (stored.AccountId == 999)
                {
                    account = new SystemAccount
                    {
                        AccountId   = 999,
                        AccountName = "Admin",
                        AccountRole = null,
                        AccountEmail = _configuration["AdminAccount:Email"]
                    };
                }
                else
                {
                    throw new AppException(Fail.EXPIRE_TOKEN);
                }
            }

            string newAccessToken  = GenerateAccessToken(account);
            string newRefreshToken = await GenerateRefreshTokenAsync(account);

            return (newAccessToken, newRefreshToken);
        }
    }
}
