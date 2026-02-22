using Assignment1.Models;
using System.Security.Claims;

namespace Assignment1.Services.Interface
{
    public interface IAuthenticationService
    {
        public SystemAccount isCorrectAccount(string username, string password);
        public string GenerateAccessToken(SystemAccount account);
        public bool IsValidAccessToken(string accessToken, out ClaimsPrincipal claimsPrincipal);

    }
}
