using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UI.Apis;
using UI.dto.request;
using UI.dto.response;

namespace UI.Controllers
{
    public class NewsArticleController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            NewsArticleResponseDto model,
            short CategoryId,
            List<int> TagIds
        )   
        {
            if (CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Vui lòng chọn danh mục");
            }
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndTags();
                return View(model);
            }
            var updateDto = new UpdateNewArticleRequestDto
            {
                NewsTitle = model.NewsTitle!,
                Headline = model.Headline,
                NewsContent = model.NewsContent,
                NewsSource = model.NewsSource,
                CategoryId = CategoryId,
                TagIds = TagIds
            };

            var response = await ApiHelper.RequestApi(
                $"http://localhost:5039/api/newsarticles/{model.NewsArticleId}",
                "PUT",
                updateDto,
                HttpContext
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Update failed";
                await LoadCategoriesAndTags();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var response = await ApiHelper.RequestApi(
                $"http://localhost:5039/api/NewsArticles/{id}",
                "GET",
                null,
                HttpContext
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Cannot load news article";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<NewsArticleResponseDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
            {
                TempData["Error"] = apiResponse?.Message ?? "Error";
                return RedirectToAction(nameof(Index));
            }

            await LoadCategoriesAndTags();
            return View(apiResponse.Data);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await ApiHelper.RequestApi(
                $"http://localhost:5039/api/NewsArticles/{id}",
                "DELETE",
                null,
                HttpContext
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Delete failed";
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAndTags();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndTags();
                return View(dto);
            }

            var response = await ApiHelper.RequestApi(
                "http://localhost:5039/api/NewsArticles",
                "POST",
                dto,
                HttpContext
            );
            Console.WriteLine(response);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Index()
        {
            return View();
        }

        private async Task LoadCategoriesAndTags()
        {
            var cateResponse = await ApiHelper.RequestApi(
                "http://localhost:5039/api/category",
                "GET",
                null,
                HttpContext
            );

            if (cateResponse.IsSuccessStatusCode)
            {
                var cateJson = await cateResponse.Content.ReadAsStringAsync();
                var cateData = JsonSerializer.Deserialize<ApiResponse<List<CategoryResponseDto>>>(
                    cateJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                ViewBag.Categories = cateData?.Data ?? new List<CategoryResponseDto>();
            }

            var tagResponse = await ApiHelper.RequestApi(
                "http://localhost:5039/api/tag",
                "GET",
                null,
                HttpContext
            );

            if (tagResponse.IsSuccessStatusCode)
            {
                var tagJson = await tagResponse.Content.ReadAsStringAsync();
                var tagData = JsonSerializer.Deserialize<ApiResponse<List<TagResponseDto>>>(
                    tagJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                ViewBag.Tags = tagData?.Data ?? new List<TagResponseDto>();
            }
        }
    }
}
