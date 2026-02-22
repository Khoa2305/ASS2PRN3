using Assignment1.DAO;
using Assignment1.Models;
using Assignment1.Repositories.Interface;

namespace Assignment1.Repositories.Imp
{
    public class AccountRepository : IAccountRepository
    {
        private readonly FunewsManagementContext _context;

        public AccountRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task<List<SystemAccount>> GetAccountsAsync()
        {
            return await AccountDAO.Instance.GetAccountsAsync(_context);
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(short id)
        {
            return await AccountDAO.Instance.GetAccountByIdAsync(_context, id);
        }

        public async Task<SystemAccount> AddAccountAsync(SystemAccount account)
        {
            await AccountDAO.Instance.AddAccountAsync(_context, account);
            return account;
        }

        public async Task UpdateAccountAsync(SystemAccount account)
        {
            await AccountDAO.Instance.UpdateAccountAsync(_context, account);
        }

        public async Task DeleteAccountAsync(short id)
        {
            await AccountDAO.Instance.DeleteAccountAsync(_context, id);
        }

        public async Task<bool> EmailExistsAsync(string email, short? excludeId = null)
        {
            return await AccountDAO.Instance.EmailExistsAsync(_context, email, excludeId);
        }

        public async Task<short> GetMaxIdAsync()
        {
            return await AccountDAO.Instance.GetMaxIdAsync(_context);
        }

        public async Task<bool> HasNewsArticlesAsync(short id)
        {
            return await AccountDAO.Instance.HasNewsArticlesAsync(_context, id);
        }
    }
}
