using Assignment1.Models;

namespace Assignment1.Repositories.Interface
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAsync(short? userId, string? entityName);
    }
}
