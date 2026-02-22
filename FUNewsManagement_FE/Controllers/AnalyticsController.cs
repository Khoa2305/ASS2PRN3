using FUNewsManagement_FE.Clients;
using FUNewsManagement_FE.dto.response;
using FUNewsManagement_FE.Models;
using Microsoft.AspNetCore.Mvc;
using UI.dto.response;

namespace FUNewsManagement_FE.Controllers
{
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsApiClient _analyticsClient;

        public AnalyticsController(IAnalyticsApiClient analyticsClient)
        {
            _analyticsClient = analyticsClient;
        }

        public async Task<IActionResult> Index()
        {
            // Only admin standard role check if needed, but assuming layout restricts sidebar
            // Could add custom auth filters here if required by assignment (e.g. [Authorize(Roles = "1")])
            var role = HttpContext.Session.GetInt32("ROLE");
            if (role != 1)
            {
                // Unauthorized if not admin
                return RedirectToAction("Index", "Home");
            }

            var vm = new AnalyticsViewModel();

            try
            {
                // Fetch dashboard stats
                var dashboardResponse = await _analyticsClient.SendAndDeserializeAsync<ApiResponse<DashboardDto>>(HttpMethod.Get, "api/analytics/dashboard");
                if (dashboardResponse != null && dashboardResponse.Success && dashboardResponse.Data != null)
                {
                    vm.DashboardMetrics = dashboardResponse.Data;
                }

                // Fetch top trending
                var trendingResponse = await _analyticsClient.SendAndDeserializeAsync<ApiResponse<List<TrendingArticleDto>>>(HttpMethod.Get, "api/analytics/trending?top=10");
                if (trendingResponse != null && trendingResponse.Success && trendingResponse.Data != null)
                {
                    vm.TrendingArticles = trendingResponse.Data;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Unable to fetch analytics data from the backend. " + ex.Message;
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var role = HttpContext.Session.GetInt32("ROLE");
            if (role != 1) return RedirectToAction("Index", "Home");

            try
            {
                var fileBytes = await _analyticsClient.DownloadExcelAsync("api/analytics/export");
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AnalyticsReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to export report: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
