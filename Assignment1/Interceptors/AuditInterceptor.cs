using Assignment1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Assignment1.Interceptors
{
    /// <summary>
    /// EF Core SaveChanges interceptor that automatically writes AuditLog rows
    /// for Add / Update / Delete operations on audited entity types.
    /// No existing service or repository is modified.
    /// </summary>
    public class AuditInterceptor : SaveChangesInterceptor
    {
        // Entity type names that should be audited
        private static readonly HashSet<string> AuditedEntities = new()
        {
            nameof(NewsArticle),
            nameof(Category),
            nameof(Tag),
            nameof(SystemAccount)
        };

        private readonly IHttpContextAccessor _httpContextAccessor;

        // Per-save snapshot of original values (captured BEFORE SaveChanges writes to DB)
        private readonly List<AuditLog> _pendingLogs = new();

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // ── 1. Snapshot BEFORE save ──────────────────────────────────────────

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _pendingLogs.Clear();

            if (eventData.Context is not FunewsManagementContext ctx) return base.SavingChangesAsync(eventData, result, cancellationToken);

            short? userId = ResolveUserId();

            foreach (var entry in ctx.ChangeTracker.Entries())
            {
                string typeName = entry.Entity.GetType().Name;
                if (!AuditedEntities.Contains(typeName)) continue;

                string? action = entry.State switch
                {
                    EntityState.Added    => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted  => "Delete",
                    _                    => null
                };
                if (action is null) continue;

                string? beforeJson = null;
                string? afterJson  = null;

                if (entry.State == EntityState.Modified)
                {
                    // Capture original (before) values
                    var before = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    beforeJson = JsonSerializer.Serialize(before);

                    var after = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    afterJson = JsonSerializer.Serialize(after);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    var before = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                    beforeJson = JsonSerializer.Serialize(before);
                }
                else if (entry.State == EntityState.Added)
                {
                    var after = entry.Properties
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                    afterJson = JsonSerializer.Serialize(after);
                }

                _pendingLogs.Add(new AuditLog
                {
                    Id         = Guid.NewGuid(),
                    UserId     = userId,
                    Action     = action,
                    EntityName = typeName,
                    BeforeData = beforeJson,
                    AfterData  = afterJson,
                    Timestamp  = DateTime.UtcNow
                });
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // ── 2. Persist logs AFTER save succeeds ─────────────────────────────

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (_pendingLogs.Count > 0 && eventData.Context is FunewsManagementContext ctx)
            {
                await ctx.AuditLogs.AddRangeAsync(_pendingLogs, cancellationToken);

                // Use the base SaveChangesAsync to avoid re-triggering this interceptor
                await ctx.Database.GetDbConnection().OpenAsync(cancellationToken);
                await ctx.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken: cancellationToken);

                _pendingLogs.Clear();
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private short? ResolveUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (short.TryParse(claim, out short id)) return id;
            return null;
        }
    }
}
