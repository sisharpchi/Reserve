using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.RecordAuditLog;

public sealed record RecordAuditLogCommand(
    Guid? TenantId,
    Guid? UserId,
    string Action,
    string EntityName,
    Guid? EntityId,
    string DetailsJson) : ICommand<AuditLogResponse>;
