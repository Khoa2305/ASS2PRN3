using Assignment1.Models;
using Assignment1.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Assignment1.Repositories.Imp
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly FunewsManagementContext _context;

        public AuditLogRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAsync(short? userId, string? entityName)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName == entityName);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
