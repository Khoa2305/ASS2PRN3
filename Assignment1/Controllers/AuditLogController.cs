using Assignment1.dto;
using Assignment1.dto.response;
using Assignment1.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment1.Controllers
{
    [Route("api/auditlog")]
    [ApiController]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogController(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        /// <summary>
        /// Retrieve audit logs with optional filtering by userId and/or entityName.
        /// entityName values: NewsArticle | Category | Tag | SystemAccount
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AuditLogResponseDto>>>> Get(
            [FromQuery] short? userId,
            [FromQuery] string? entityName)
        {
            var logs = await _auditLogRepository.GetAsync(userId, entityName);

            var dtos = logs.Select(a => new AuditLogResponseDto
            {
                Id         = a.Id,
                UserId     = a.UserId,
                Action     = a.Action,
                EntityName = a.EntityName,
                BeforeData = a.BeforeData,
                AfterData  = a.AfterData,
                Timestamp  = a.Timestamp
            }).ToList();

            return Ok(new ApiResponse<List<AuditLogResponseDto>>
            {
                Success = true,
                Message = $"Found {dtos.Count} audit log(s)",
                Data    = dtos
            });
        }
    }
}
