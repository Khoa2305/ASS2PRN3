using Assignment1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
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

        // Associates pending audit logs with each DbContext instance (GC-friendly, thread-safe)
        private static readonly ConditionalWeakTable<DbContext, List<AuditLog>> _pendingTable = new();

        // Re-entrancy flag — prevents audit log saves from triggering a second audit
        [ThreadStatic]
        private static bool _isSaving;

        private readonly IHttpContextAccessor _httpContextAccessor;

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
            if (_isSaving || eventData.Context is not FunewsManagementContext ctx)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var pending = new List<AuditLog>();
            _pendingTable.AddOrUpdate(ctx, pending);

            short? userId = ResolveUserId();
            var jsonOpts  = new JsonSerializerOptions { WriteIndented = false };

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
                    beforeJson = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue), jsonOpts);
                    afterJson = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue), jsonOpts);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    beforeJson = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue), jsonOpts);
                }
                else // Added
                {
                    afterJson = JsonSerializer.Serialize(
                        entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue), jsonOpts);
                }

                pending.Add(new AuditLog
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
            if (!_isSaving &&
                eventData.Context is FunewsManagementContext ctx &&
                _pendingTable.TryGetValue(ctx, out var pending) &&
                pending.Count > 0)
            {
                _isSaving = true;
                try
                {
                    await ctx.AuditLogs.AddRangeAsync(pending, cancellationToken);
                    await ctx.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    _isSaving = false;
                    _pendingTable.Remove(ctx);
                }
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private short? ResolveUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return short.TryParse(claim, out short id) ? id : null;
        }
    }
}
