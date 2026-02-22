using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;

namespace Assignment1.Mapper
{
    public class AccountMapHelper
    {
        public static AccountResponseDto OriginToResponseDto(SystemAccount account)
        {
            if (account == null) { return null; }
            return new AccountResponseDto
            {
                Id = account.AccountId,
                Name = account.AccountName,
                Email = account.AccountEmail,
                Role = account.AccountRole == 1 ? "Staff" : "Lecturer"
            };
        }
        public static AccountDetailResponseDto OriginToDetailDto(SystemAccount systemAccount)
        {
            if (systemAccount == null) { return null; }
            return new AccountDetailResponseDto
            {
                Id = systemAccount.AccountId,
                Name = systemAccount.AccountName,
                Email = systemAccount.AccountEmail,
                Password = systemAccount.AccountPassword,
                Role = systemAccount.AccountRole.Value
            };
        }
        public static void MapUpdateToOrigin(UpdateAccountRequestDto updateAccountRequestDto, SystemAccount account)
        {
            account.AccountName = updateAccountRequestDto.Name;
            account.AccountEmail = updateAccountRequestDto.Email;
            account.AccountPassword = updateAccountRequestDto.Password;
            account.AccountRole = updateAccountRequestDto.Role;
        }
        public static SystemAccount OriginFromCreateDto(AccountCreatRequestDto accountCreatRequestDto)
        {
            if (accountCreatRequestDto == null) { throw new ArgumentNullException(); }
            return new SystemAccount
            {

                AccountEmail = accountCreatRequestDto.AccountEmail,
                AccountName = accountCreatRequestDto.AccountName,
                AccountRole = accountCreatRequestDto.AccountRole,
                AccountPassword = accountCreatRequestDto.AccountPassword,
            };
        }
    }
}
