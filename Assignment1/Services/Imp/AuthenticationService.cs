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
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAccountRepository accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
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
                issuer:            issuer,
                audience:          aud,
                claims:            claims,
                expires:           DateTime.UtcNow.AddMinutes(expMin),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

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
            {
                throw new AppException(Fail.WRONG_ACCOUNT);
            }
            return account;
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
    }
}
