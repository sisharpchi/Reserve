using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string Action,
    string EntityName,
    Guid? EntityId,
    string DetailsJson,
    DateTime OccurredOnUtc)
{
    public static AuditLogResponse FromAuditLog(AuditLog log)
    {
        return new AuditLogResponse(
            log.Id,
            log.TenantId,
            log.UserId,
            log.Action,
            log.EntityName,
            log.EntityId,
            log.DetailsJson,
            log.OccurredOnUtc);
    }
}
