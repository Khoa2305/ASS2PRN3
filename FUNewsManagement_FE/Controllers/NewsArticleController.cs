using Microsoft.AspNetCore.Mvc;
using FUNewsManagement_FE.Clients;
using UI.dto.request;
using UI.dto.response;

namespace UI.Controllers
{
    public class NewsArticleController : Controller
    {
        private readonly ICoreApiClient _coreApi;

        public NewsArticleController(ICoreApiClient coreApi)
        {
            _coreApi = coreApi;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [FUNewsManagement_FE.Filters.OfflineModeGuard]
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

            var response = await _coreApi.SendAsync(HttpMethod.Put, $"api/newsarticles/{model.NewsArticleId}", updateDto);

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
            var apiResponse = await _coreApi.SendAndDeserializeAsync<ApiResponse<NewsArticleResponseDto>>(
                HttpMethod.Get, 
                $"api/NewsArticles/{id}");

            if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
            {
                TempData["Error"] = apiResponse?.Message ?? "Error loading news article";
                return RedirectToAction(nameof(Index));
            }

            await LoadCategoriesAndTags();
            return View(apiResponse.Data);
        }

        [HttpPost]
        [FUNewsManagement_FE.Filters.OfflineModeGuard]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _coreApi.SendAsync(HttpMethod.Delete, $"api/NewsArticles/{id}");

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
        [FUNewsManagement_FE.Filters.OfflineModeGuard]
        public async Task<IActionResult> Create(NewsArticleRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndTags();
                return View(dto);
            }

            var response = await _coreApi.SendAsync(HttpMethod.Post, "api/NewsArticles", dto);
            Console.WriteLine(response);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            return View();
        }

        private async Task LoadCategoriesAndTags()
        {
            var cateData = await _coreApi.SendAndDeserializeAsync<ApiResponse<List<CategoryResponseDto>>>(
                HttpMethod.Get, 
                "api/category");

            if (cateData != null && cateData.Success)
            {
                ViewBag.Categories = cateData.Data ?? new List<CategoryResponseDto>();
            }
            else
            {
                ViewBag.Categories = new List<CategoryResponseDto>();
            }

            var tagData = await _coreApi.SendAndDeserializeAsync<ApiResponse<List<TagResponseDto>>>(
                HttpMethod.Get, 
                "api/tag");

            if (tagData != null && tagData.Success)
            {
                ViewBag.Tags = tagData.Data ?? new List<TagResponseDto>();
            }
            else
            {
                ViewBag.Tags = new List<TagResponseDto>();
            }
        }
    }
}
