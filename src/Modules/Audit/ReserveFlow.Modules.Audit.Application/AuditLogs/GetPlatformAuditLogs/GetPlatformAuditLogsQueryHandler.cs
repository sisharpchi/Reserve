using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.GetPlatformAuditLogs;

public sealed class GetPlatformAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IQueryHandler<GetPlatformAuditLogsQuery, IReadOnlyList<AuditLogResponse>>
{
    public async Task<IReadOnlyList<AuditLogResponse>> Handle(
        GetPlatformAuditLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        int limit = Math.Clamp(query.Limit, 1, 200);
        IReadOnlyList<AuditLog> logs = await auditLogRepository.GetRecentAsync(limit, cancellationToken);

        return logs
            .Select(AuditLogResponse.FromAuditLog)
            .ToArray();
    }
}
