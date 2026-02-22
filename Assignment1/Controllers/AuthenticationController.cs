using Assignment1.dto;
using Assignment1.dto.request;
using Assignment1.dto.response;
using Assignment1.Models;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Http;
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
        [HttpPost("login")]
        public ActionResult<ApiResponse<Tokens>> Login([FromBody] LoginRequestDto requestDto)
        {
            SystemAccount account =
                _authenticationService.isCorrectAccount(requestDto.Email, requestDto.Password);

            string accessToken =
                _authenticationService.GenerateAccessToken(account);

            return Ok(new ApiResponse<Tokens>
            {
                Success = true,
                Message = "Login success",
                Data = new Tokens
                {
                    AccessToken = accessToken,
                    Role = account.AccountRole ?? 0 // ✅ Admin = 0
                }
            });
        }

    }
}
