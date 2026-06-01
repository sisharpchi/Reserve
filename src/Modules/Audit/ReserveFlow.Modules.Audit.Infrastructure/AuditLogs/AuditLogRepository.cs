using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;
using ReserveFlow.Modules.Audit.Infrastructure.Database;

namespace ReserveFlow.Modules.Audit.Infrastructure.AuditLogs;

internal sealed class AuditLogRepository(AuditDbContext dbContext) : IAuditLogRepository
{
    public void Insert(AuditLog log)
    {
        dbContext.AuditLogs.Add(log);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(log => log.OccurredOnUtc)
            .Take(limit)
            .ToArrayAsync(cancellationToken);
    }
}
