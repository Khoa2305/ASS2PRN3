namespace FUNewsManagement_FE.Services
{
    public interface ITokenService
    {
        string? GetAccessToken();
        string? GetRefreshToken();
        bool IsTokenExpired(string token);
        Task<string?> RefreshTokenAsync();
    }
}
