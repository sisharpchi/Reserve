namespace ReserveFlow.Modules.Audit.Presentation;

internal sealed record RecordAuditLogRequest(
    Guid? TenantId,
    Guid? UserId,
    string Action,
    string EntityName,
    Guid? EntityId,
    string DetailsJson);
