using FUNewsManagement_AnalyticsAPI.Dto;
using FUNewsManagement_AnalyticsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_AnalyticsAPI.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        // ── Bearer token forwarding helper ────────────────────────────────────
        private string? Bearer() =>
            Request.Headers.TryGetValue("Authorization", out var v)
                ? v.ToString().Replace("Bearer ", "").Trim()
                : null;

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/analytics/dashboard
        // ?from=2024-01-01&to=2024-12-31&category=Tech&author=John
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string?   category,
            [FromQuery] string?   author)
        {
            var filter   = new AnalyticsFilter { From = from, To = to, Category = category, Author = author };
            var result   = await _analytics.GetDashboardAsync(filter, Bearer());
            return Ok(new { success = true, data = result });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/analytics/trending
        // ?top=10&from=&to=&category=&author=
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("trending")]
        public async Task<IActionResult> Trending(
            [FromQuery] int       top      = 10,
            [FromQuery] DateTime? from     = null,
            [FromQuery] DateTime? to       = null,
            [FromQuery] string?   category = null,
            [FromQuery] string?   author   = null)
        {
            if (top < 1 || top > 100) top = 10;
            var filter = new AnalyticsFilter { From = from, To = to, Category = category, Author = author };
            var result = await _analytics.GetTrendingAsync(filter, top, Bearer());
            return Ok(new { success = true, data = result });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/analytics/export
        // ?from=&to=&category=&author=
        // Returns an .xlsx file download
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string?   category,
            [FromQuery] string?   author)
        {
            var filter = new AnalyticsFilter { From = from, To = to, Category = category, Author = author };
            var bytes  = await _analytics.ExportExcelAsync(filter, Bearer());

            var filename = $"FUNews_Analytics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                filename);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Route note: recommend sits outside /api/analytics intentionally
    // ─────────────────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/recommend")]
    public class RecommendController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;

        public RecommendController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        private string? Bearer() =>
            Request.Headers.TryGetValue("Authorization", out var v)
                ? v.ToString().Replace("Bearer ", "").Trim()
                : null;

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/recommend/{id}
        // ?top=5
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> Recommend([FromRoute] string id, [FromQuery] int top = 5)
        {
            if (top < 1 || top > 20) top = 5;
            var result = await _analytics.GetRecommendationsAsync(id, top, Bearer());
            return Ok(new { success = true, data = result });
        }
    }
}
