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
            string jwt_signer_key = _configuration["jwt_signer_key"];
            int access_token_duration = int.Parse(_configuration["access_token_duration"]);
            var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key));
            var creds = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256);

            // Requirement: 1 = Staff, 2 = Lecturer, Admin = Admin
            string role = "Admin";
            if (account.AccountRole == 1) role = "1";
            else if (account.AccountRole == 2) role = "2";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.AccountEmail ?? ""),
                new Claim(ClaimTypes.Role, role),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "ass",
                audience: "member",
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(access_token_duration),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public SystemAccount isCorrectAccount(string username, string password)
        {
            string adminEmail = _configuration["email"];
            string adminPassword = _configuration["password"];
            if (username.Equals(adminEmail) && password.Equals(adminPassword))
            {
                return new SystemAccount
                {
                    AccountEmail = adminEmail,
                    AccountRole = null, // Admin role is null to distinguish from Staff (1) and Lecturer (2)
                    AccountId = 999,
                    AccountName = "Admin",
                    AccountPassword = password,
                };
            }

            // Using repository to find account
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
            string jwt_signer_key = _configuration["jwt_signer_key"];
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "ass",
                ValidateAudience = true,
                ValidAudience = "member",
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt_signer_key)),
                ClockSkew = TimeSpan.Zero,
            };
            try
            {
                SecurityToken? token;
                claimsPrincipal = securityTokenHandler.ValidateToken(accessToken, parameters, out token);
                return true;
            }
            catch
            {
                claimsPrincipal = null;
                return false;
            }
        }
    }
}
