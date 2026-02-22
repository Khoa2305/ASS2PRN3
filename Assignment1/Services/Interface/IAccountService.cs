using Assignment1.Models;

namespace Assignment1.Services.Interface
{
    public interface IAccountService
    {
        public Task<List<SystemAccount>> GetAccountsAsync();
        public Task<SystemAccount> GetAccountByIdAsync(int id);
        public Task UpdateAccountAsync(SystemAccount account);
        public Task DeleteAccountAsync(int id);
        public Task<SystemAccount> AddAsync(SystemAccount account);
        public Task<short> GetValidId();
        public Task ChangePasswordAsync(int accountId, string currentPassword, string newPassword);
    }
}
