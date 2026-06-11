using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs;

public interface IAuditLogRepository
{
    void Insert(AuditLog log);

    Task<PagedResult<AuditLog>> GetRecentAsync(
        PageRequest pageRequest,
        Guid? tenantId,
        string? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
