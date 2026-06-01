using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs;

public interface IAuditLogRepository
{
    void Insert(AuditLog log);

    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}
