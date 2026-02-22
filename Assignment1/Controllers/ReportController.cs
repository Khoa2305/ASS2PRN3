using Assignment1.dto;
using Assignment1.dto.response;
using Assignment1.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Assignment1.Constant;
using Assignment1.CustomException;
using Microsoft.AspNetCore.Authorization;

namespace Assignment1.Controllers
{
    [Route("api/report")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<ReportResponseDto>>> GetReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var report = await _reportService.GetReportAsync(startDate, endDate);
            return Ok(new ApiResponse<ReportResponseDto>
            {
                Success = true,
                Message = "Get report success",
                Data = report
            });
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var fileBytes = await _reportService.ExportToExcelAsync(startDate, endDate);
            return File(
                fileBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"Report_{DateTime.Now:yyyyMMdd}.xlsx");
        }
    }
}
