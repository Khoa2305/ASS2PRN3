using Assignment1.dto.response;
using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Assignment1.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Assignment1.Services.Imp
{
    public class ReportService : IReportService
    {
        private readonly INewsArticleRepository _newsArticleRepository;

        public ReportService(INewsArticleRepository newsArticleRepository)
        {
            _newsArticleRepository = newsArticleRepository;
        }

        public async Task<ReportResponseDto> GetReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _newsArticleRepository.GetNewsArticlesQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.CreatedDate <= endDate.Value);
            }

            // Group by Category
            var byCategory = await query
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new GroupedStatDto
                {
                    Key = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            // Group by Author
            var byAuthor = await query
                .GroupBy(x => x.CreatedBy.AccountName)
                .Select(g => new GroupedStatDto
                {
                    Key = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            // Status counts
            var activeCount = await query.CountAsync(x => x.NewsStatus == true);
            var inactiveCount = await query.CountAsync(x => x.NewsStatus != true);

            return new ReportResponseDto
            {
                ByCategory = byCategory,
                ByAuthor = byAuthor,
                TotalActive = activeCount,
                TotalInactive = inactiveCount
            };
        }

        public async Task<byte[]> ExportToExcelAsync(DateTime? startDate, DateTime? endDate)
        {
            var report = await GetReportAsync(startDate, endDate);

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Article Report");

                // Header
                worksheet.Cell(1, 1).Value = "Article Report";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                
                string period = $"Period: {(startDate?.ToString("dd/MM/yyyy") ?? "Start")} - {(endDate?.ToString("dd/MM/yyyy") ?? "End")}";
                worksheet.Cell(2, 1).Value = period;

                // Summary
                worksheet.Cell(4, 1).Value = "Summary";
                worksheet.Cell(4, 1).Style.Font.Bold = true;
                worksheet.Cell(5, 1).Value = "Total Active:";
                worksheet.Cell(5, 2).Value = report.TotalActive;
                worksheet.Cell(6, 1).Value = "Total Inactive:";
                worksheet.Cell(6, 2).Value = report.TotalInactive;

                // By Category
                int row = 8;
                worksheet.Cell(row, 1).Value = "By Category";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                row++;
                worksheet.Cell(row, 1).Value = "Category Name";
                worksheet.Cell(row, 2).Value = "Count";
                worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
                foreach (var item in report.ByCategory)
                {
                    row++;
                    worksheet.Cell(row, 1).Value = item.Key;
                    worksheet.Cell(row, 2).Value = item.Count;
                }

                // By Author
                row += 2;
                worksheet.Cell(row, 1).Value = "By Author";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                row++;
                worksheet.Cell(row, 1).Value = "Author Name";
                worksheet.Cell(row, 2).Value = "Count";
                worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
                foreach (var item in report.ByAuthor)
                {
                    row++;
                    worksheet.Cell(row, 1).Value = item.Key;
                    worksheet.Cell(row, 2).Value = item.Count;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
