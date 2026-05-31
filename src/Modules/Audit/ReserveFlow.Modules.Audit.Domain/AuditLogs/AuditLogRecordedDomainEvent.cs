using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Audit.Domain.AuditLogs;

public sealed record AuditLogRecordedDomainEvent(
    Guid AuditLogId,
    Guid? TenantId,
    string Action,
    string EntityName) : DomainEvent;
