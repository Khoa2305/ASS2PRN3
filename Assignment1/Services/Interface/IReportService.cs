using Assignment1.dto.response;

namespace Assignment1.Services.Interface
{
    public interface IReportService
    {
        Task<ReportResponseDto> GetReportAsync(DateTime? startDate, DateTime? endDate);
        Task<byte[]> ExportToExcelAsync(DateTime? startDate, DateTime? endDate);
    }
}
