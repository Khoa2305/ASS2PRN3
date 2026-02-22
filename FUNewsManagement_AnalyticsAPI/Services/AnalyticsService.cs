using ClosedXML.Excel;
using FUNewsManagement_AnalyticsAPI.Client;
using FUNewsManagement_AnalyticsAPI.Dto;

namespace FUNewsManagement_AnalyticsAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ICoreApiClient _coreApi;

        public AnalyticsService(ICoreApiClient coreApi)
        {
            _coreApi = coreApi;
        }

        // ── 1. Dashboard ──────────────────────────────────────────────────────

        public async Task<DashboardDto> GetDashboardAsync(AnalyticsFilter filter, string? bearerToken = null)
        {
            var articles = await _coreApi.GetAllArticlesAsync(bearerToken);
            var filtered = ApplyFilter(articles, filter);

            var byCategory = filtered
                .GroupBy(a => a.CategoryName ?? "Uncategorized")
                .Select(g => new CategoryStatDto
                {
                    CategoryName = g.Key,
                    Count        = g.Count(),
                    TotalViews   = g.Sum(a => a.ViewCount)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var byAuthor = filtered
                .GroupBy(a => a.CreatedByName ?? "Unknown")
                .Select(g => new AuthorStatDto
                {
                    AuthorName = g.Key,
                    Count      = g.Count(),
                    TotalViews = g.Sum(a => a.ViewCount)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            var byDay = filtered
                .Where(a => a.CreatedDate.HasValue)
                .GroupBy(a => a.CreatedDate!.Value.ToString("yyyy-MM-dd"))
                .Select(g => new DailyStatDto { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            return new DashboardDto
            {
                TotalArticles     = filtered.Count,
                PublishedArticles = filtered.Count(a => a.NewsStatus == true),
                DraftArticles     = filtered.Count(a => a.NewsStatus != true),
                TotalViewCount    = filtered.Sum(a => a.ViewCount),
                ByCategory        = byCategory,
                ByAuthor          = byAuthor,
                ByDay             = byDay
            };
        }

        // ── 2. Trending ───────────────────────────────────────────────────────

        public async Task<List<TrendingArticleDto>> GetTrendingAsync(AnalyticsFilter filter, int top = 10, string? bearerToken = null)
        {
            // Pull a larger pool from CoreAPI trending, then apply local filters
            var raw = await _coreApi.GetTrendingArticlesAsync(top * 4, bearerToken);
            var filtered = ApplyFilter(raw, filter);

            return filtered
                .OrderByDescending(a => a.ViewCount)
                .Take(top)
                .Select(a => new TrendingArticleDto
                {
                    NewsArticleId = a.NewsArticleId,
                    NewsTitle     = a.NewsTitle,
                    CategoryName  = a.CategoryName,
                    AuthorName    = a.CreatedByName,
                    ViewCount     = a.ViewCount
                })
                .ToList();
        }

        // ── 3. Recommendations ────────────────────────────────────────────────

        public async Task<RecommendDto> GetRecommendationsAsync(string id, int top = 5, string? bearerToken = null)
        {
            var target = await _coreApi.GetArticleByIdAsync(id, bearerToken);
            if (target == null)
                return new RecommendDto { TargetArticleId = id };

            var all = await _coreApi.GetAllArticlesAsync(bearerToken);

            var targetTagSet = target.Tags.ToHashSet();

            var recommendations = all
                .Where(a => a.NewsArticleId != id && a.NewsStatus == true)
                .Select(a =>
                {
                    int sharedTags     = a.Tags.Count(t => targetTagSet.Contains(t));
                    bool sameCategory  = a.CategoryId.HasValue && a.CategoryId == target.CategoryId;
                    double score       = (sharedTags * 3.0)
                                       + (sameCategory ? 2.0 : 0)
                                       + (a.ViewCount / 100.0);

                    return new RecommendItemDto
                    {
                        NewsArticleId = a.NewsArticleId,
                        NewsTitle     = a.NewsTitle,
                        CategoryName  = a.CategoryName,
                        SharedTags    = sharedTags,
                        ViewCount     = a.ViewCount,
                        Score         = Math.Round(score, 2)
                    };
                })
                .Where(r => r.Score > 0)
                .OrderByDescending(r => r.Score)
                .Take(top)
                .ToList();

            return new RecommendDto
            {
                TargetArticleId = id,
                TargetTitle     = target.NewsTitle,
                Recommendations = recommendations
            };
        }

        // ── 4. Excel Export ───────────────────────────────────────────────────

        public async Task<byte[]> ExportExcelAsync(AnalyticsFilter filter, string? bearerToken = null)
        {
            var articles = await _coreApi.GetAllArticlesAsync(bearerToken);
            var filtered = ApplyFilter(articles, filter);
            var dashboard = await GetDashboardAsync(filter, bearerToken);

            using var workbook = new XLWorkbook();

            // ── Sheet 1: Articles ─────────────────────────────────────────────
            var ws1 = workbook.Worksheets.Add("Articles");
            var headers = new[] { "ID", "Title", "Category", "Author", "Status", "Views", "Created Date" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws1.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.SteelBlue;
                cell.Style.Font.FontColor       = XLColor.White;
            }

            int row = 2;
            foreach (var a in filtered)
            {
                ws1.Cell(row, 1).Value = a.NewsArticleId;
                ws1.Cell(row, 2).Value = a.NewsTitle ?? "";
                ws1.Cell(row, 3).Value = a.CategoryName ?? "";
                ws1.Cell(row, 4).Value = a.CreatedByName ?? "";
                ws1.Cell(row, 5).Value = a.NewsStatus == true ? "Published" : "Draft";
                ws1.Cell(row, 6).Value = a.ViewCount;
                ws1.Cell(row, 7).Value = a.CreatedDate.HasValue
                    ? a.CreatedDate.Value.ToString("yyyy-MM-dd")
                    : "";
                row++;
            }
            ws1.Columns().AdjustToContents();

            // ── Sheet 2: Dashboard Summary ────────────────────────────────────
            var ws2 = workbook.Worksheets.Add("Dashboard");
            ws2.Cell(1, 1).Value = "Metric";
            ws2.Cell(1, 2).Value = "Value";
            ws2.Cell(1, 1).Style.Font.Bold = true;
            ws2.Cell(1, 2).Style.Font.Bold = true;

            var summary = new (string Key, object Value)[]
            {
                ("Total Articles",      dashboard.TotalArticles),
                ("Published",           dashboard.PublishedArticles),
                ("Drafts",              dashboard.DraftArticles),
                ("Total Views",         dashboard.TotalViewCount),
                ("Filter: From",        filter.From?.ToString("yyyy-MM-dd") ?? "-"),
                ("Filter: To",          filter.To?.ToString("yyyy-MM-dd") ?? "-"),
                ("Filter: Category",    filter.Category ?? "-"),
                ("Filter: Author",      filter.Author ?? "-"),
            };

            for (int i = 0; i < summary.Length; i++)
            {
                ws2.Cell(i + 2, 1).Value = summary[i].Key;
                ws2.Cell(i + 2, 2).Value = summary[i].Value.ToString();
            }
            ws2.Columns().AdjustToContents();

            // ── Sheet 3: Category Stats ───────────────────────────────────────
            var ws3 = workbook.Worksheets.Add("By Category");
            ws3.Cell(1, 1).Value = "Category";
            ws3.Cell(1, 2).Value = "Count";
            ws3.Cell(1, 3).Value = "Total Views";
            for (int c = 1; c <= 3; c++) ws3.Cell(1, c).Style.Font.Bold = true;

            int catRow = 2;
            foreach (var cat in dashboard.ByCategory)
            {
                ws3.Cell(catRow, 1).Value = cat.CategoryName;
                ws3.Cell(catRow, 2).Value = cat.Count;
                ws3.Cell(catRow, 3).Value = cat.TotalViews;
                catRow++;
            }
            ws3.Columns().AdjustToContents();

            // ── Sheet 4: Author Stats ─────────────────────────────────────────
            var ws4 = workbook.Worksheets.Add("By Author");
            ws4.Cell(1, 1).Value = "Author";
            ws4.Cell(1, 2).Value = "Count";
            ws4.Cell(1, 3).Value = "Total Views";
            for (int c = 1; c <= 3; c++) ws4.Cell(1, c).Style.Font.Bold = true;

            int authRow = 2;
            foreach (var auth in dashboard.ByAuthor)
            {
                ws4.Cell(authRow, 1).Value = auth.AuthorName;
                ws4.Cell(authRow, 2).Value = auth.Count;
                ws4.Cell(authRow, 3).Value = auth.TotalViews;
                authRow++;
            }
            ws4.Columns().AdjustToContents();

            // serialise to byte[]
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Filter helper ─────────────────────────────────────────────────────

        private static List<NewsArticleDto> ApplyFilter(List<NewsArticleDto> articles, AnalyticsFilter filter)
        {
            var q = articles.AsEnumerable();

            if (filter.From.HasValue)
                q = q.Where(a => a.CreatedDate >= filter.From.Value);

            if (filter.To.HasValue)
                q = q.Where(a => a.CreatedDate <= filter.To.Value);

            if (!string.IsNullOrWhiteSpace(filter.Category))
                q = q.Where(a => a.CategoryName != null &&
                                 a.CategoryName.Contains(filter.Category, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(filter.Author))
                q = q.Where(a => a.CreatedByName != null &&
                                 a.CreatedByName.Contains(filter.Author, StringComparison.OrdinalIgnoreCase));

            return q.ToList();
        }
    }
}
