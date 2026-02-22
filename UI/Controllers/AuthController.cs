using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UI.Apis;
using UI.dto.request;
using UI.dto.response;

namespace UI.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginRequestDto requestDto)
        {
            var response = await ApiHelper.RequestApi(
                "http://localhost:5039/api/auth/login",
                "POST",
                requestDto,
                HttpContext
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Login failed");
                return View(requestDto);
            }

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
            {
                ModelState.AddModelError("", apiResponse?.Message ?? "Login error");
                return View(requestDto);
            }

            // ===== SET SESSION =====
            HttpContext.Session.SetString(
                "ACCESS_TOKEN",
                apiResponse.Data.AccessToken
            );

            HttpContext.Session.SetInt32(
                "ROLE",
                apiResponse.Data.Role
            );
            // =======================

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // xóa luôn ROLE + TOKEN
            return RedirectToAction("Index", "Home");
        }
    }
}
