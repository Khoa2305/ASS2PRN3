using Assignment1.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.DAO
{
    public class AccountDAO
    {
        private static AccountDAO instance = null!;
        private static readonly object instanceLock = new object();

        private AccountDAO() { }

        public static AccountDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AccountDAO();
                    }
                    return instance;
                }
            }
        }

        public async Task<List<SystemAccount>> GetAccountsAsync(FunewsManagementContext context)
        {
            return await context.SystemAccounts.ToListAsync();
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(FunewsManagementContext context, short id)
        {
            return await context.SystemAccounts.FindAsync(id);
        }

        public async Task AddAccountAsync(FunewsManagementContext context, SystemAccount account)
        {
            await context.SystemAccounts.AddAsync(account);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAccountAsync(FunewsManagementContext context, SystemAccount account)
        {
            context.SystemAccounts.Update(account);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(FunewsManagementContext context, short id)
        {
            var account = await context.SystemAccounts.FindAsync(id);
            if (account != null)
            {
                context.SystemAccounts.Remove(account);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(FunewsManagementContext context, string email, short? excludeId = null)
        {
            return await context.SystemAccounts.AnyAsync(s => s.AccountEmail == email && (!excludeId.HasValue || s.AccountId != excludeId.Value));
        }

        public async Task<short> GetMaxIdAsync(FunewsManagementContext context)
        {
             var maxId = await context.SystemAccounts
                .Select(x => (short?)x.AccountId)
                .MaxAsync();
             return maxId ?? 0;
        }

        public async Task<bool> HasNewsArticlesAsync(FunewsManagementContext context, short id)
        {
            return await context.NewsArticles.AnyAsync(s => s.CreatedById == id);
        }
    }
}
