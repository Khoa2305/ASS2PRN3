using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Mapper;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Assignment1.Constant;
using Assignment1.CustomException;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<AccountResponseDto>>>> GetAccount()
        {
            List<SystemAccount > accounts = await _accountService.GetAccountsAsync();
            return new ApiResponse<List<AccountResponseDto>>
            {
                Success = true,
                Message = "Get accounts success",
                Data = accounts.Select(s => AccountMapHelper.OriginToResponseDto(s)).ToList()
            };
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<AccountDetailResponseDto>>> GetDetail([FromRoute] int id)
        {
            SystemAccount account = await _accountService.GetAccountByIdAsync(id);
            return new ApiResponse<AccountDetailResponseDto>
            {
                Data = AccountMapHelper.OriginToDetailDto(account),
                Success = true,
                Message = "Get account detail succes"
            };
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<AccountDetailResponseDto>>> GetMe()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                SystemAccount account = await _accountService.GetAccountByIdAsync(userId);
                return new ApiResponse<AccountDetailResponseDto>
                {
                    Data = AccountMapHelper.OriginToDetailDto(account),
                    Success = true,
                    Message = "Get personal profile success"
                };
            }
            return Unauthorized();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateAccount([FromBody] UpdateAccountRequestDto requestDto)
        {
            SystemAccount account =await  _accountService.GetAccountByIdAsync(requestDto.Id);
            AccountMapHelper.MapUpdateToOrigin(requestDto, account);
            await _accountService.UpdateAccountAsync(account);
            return new ApiResponse<object>
            {
                Success = true,
                Message = "Update account success"
            };
        }

        [HttpPut("me/profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateProfileRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                SystemAccount account = await _accountService.GetAccountByIdAsync(userId);
                account.AccountName = requestDto.AccountName;
                account.AccountEmail = requestDto.AccountEmail;
                await _accountService.UpdateAccountAsync(account);
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Update profile success"
                };
            }
            return Unauthorized();
        }

        [HttpPost("me/change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequestDto requestDto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(userIdClaim, out short userId))
            {
                await _accountService.ChangePasswordAsync(userId, requestDto.CurrentPassword, requestDto.NewPassword);
                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Change password success"
                };
            }
            return Unauthorized();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAccount([FromRoute] int id) 
        {
            await _accountService.DeleteAccountAsync(id);
            return new ApiResponse<object>()
            {
                Success = true,
                Message = "Delete success"
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<SystemAccount>>> AddAccount([FromBody] AccountCreatRequestDto requestDto)
        {
            SystemAccount account = AccountMapHelper.OriginFromCreateDto(requestDto);
            account.AccountId =await  _accountService.GetValidId();
            await _accountService.AddAsync(account);
            return new ApiResponse<SystemAccount>
            {
                Data = account,
                Message = "Add account success",
                Success = true
            };
        }
    }
}
