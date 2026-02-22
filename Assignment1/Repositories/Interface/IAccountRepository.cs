using Assignment1.Models;

namespace Assignment1.Repositories.Interface
{
    public interface IAccountRepository
    {
        Task<List<SystemAccount>> GetAccountsAsync();
        Task<SystemAccount?> GetAccountByIdAsync(short id);
        Task<SystemAccount> AddAccountAsync(SystemAccount account);
        Task UpdateAccountAsync(SystemAccount account);
        Task DeleteAccountAsync(short id);
        Task<bool> EmailExistsAsync(string email, short? excludeId = null);
        Task<short> GetMaxIdAsync();
        Task<bool> HasNewsArticlesAsync(short id);
    }
}
