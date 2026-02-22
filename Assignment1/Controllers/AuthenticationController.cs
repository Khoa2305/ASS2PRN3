using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>Login — returns access_token + refresh_token</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<Tokens>>> Login([FromBody] LoginRequestDto requestDto)
        {
            SystemAccount account =
                _authenticationService.isCorrectAccount(requestDto.Email, requestDto.Password);

            string accessToken  = _authenticationService.GenerateAccessToken(account);
            string refreshToken = await _authenticationService.GenerateRefreshTokenAsync(account);

            return Ok(new ApiResponse<Tokens>
            {
                Success = true,
                Message = "Login success",
                Data = new Tokens
                {
                    AccessToken  = accessToken,
                    RefreshToken = refreshToken,
                    Role         = account.AccountRole ?? 0
                }
            });
        }

        /// <summary>Refresh — exchange a valid refresh token for a new token pair</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<Tokens>>> Refresh([FromBody] RefreshTokenRequestDto requestDto)
        {
            var (newAccessToken, newRefreshToken) =
                await _authenticationService.RefreshAsync(requestDto.RefreshToken);

            return Ok(new ApiResponse<Tokens>
            {
                Success = true,
                Message = "Token refreshed",
                Data = new Tokens
                {
                    AccessToken  = newAccessToken,
                    RefreshToken = newRefreshToken
                }
            });
        }
    }
}
