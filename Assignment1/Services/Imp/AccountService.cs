using Assignment1.Constant;
using Assignment1.CustomException;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;

namespace Assignment1.Services.Imp
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<SystemAccount> AddAsync(SystemAccount account)
        {
            if (await _accountRepository.EmailExistsAsync(account.AccountEmail))
            {
                throw new AppException(Fail.EMAIL_IS_EXISTED);
            }
            return await _accountRepository.AddAccountAsync(account);
        }

        public async Task DeleteAccountAsync(int id)
        {
            if (await _accountRepository.HasNewsArticlesAsync((short)id))
            {
                throw new AppException(Fail.ARTICAL_OWN_ACCOUTN);
            }
            await _accountRepository.DeleteAccountAsync((short)id);
        }

        public async Task<SystemAccount> GetAccountByIdAsync(int id)
        {
            var account = await _accountRepository.GetAccountByIdAsync((short)id);
            if (account == null)
            {
                throw new AppException(Fail.FAIL_GET_ACCOUNT_DETAIL);
            }
            return account;
        }

        public async Task<List<SystemAccount>> GetAccountsAsync()
        {
            try
            {
                return await _accountRepository.GetAccountsAsync();
            }
            catch
            {
                throw new AppException(Fail.FAIL_GET_ACCOUNTS);
            }
        }

        public async Task<short> GetValidId()
        {
            short maxId = await _accountRepository.GetMaxIdAsync();
            return (short)(maxId + 1);
        }

        public async Task UpdateAccountAsync(SystemAccount account)
        {
            if (await _accountRepository.EmailExistsAsync(account.AccountEmail, account.AccountId))
            {
                throw new AppException(Fail.EMAIL_IS_EXISTED);
            }
            await _accountRepository.UpdateAccountAsync(account);
        }

        public async Task ChangePasswordAsync(int accountId, string currentPassword, string newPassword)
        {
            var account = await _accountRepository.GetAccountByIdAsync((short)accountId);
            if (account == null) throw new AppException(Fail.FAIL_GET_ACCOUNT_DETAIL);

            if (account.AccountPassword != currentPassword)
            {
                throw new AppException(Fail.WRONG_PASSWORD);
            }

            account.AccountPassword = newPassword;
            await _accountRepository.UpdateAccountAsync(account);
        }
    }
}
