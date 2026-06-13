using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.GetPlatformAuditLogs;

public sealed record GetPlatformAuditLogsQuery(
    int? PageNumber,
    int? PageSize,
    Guid? TenantId,
    string? Action,
    string? EntityName,
    DateTime? FromUtc,
    DateTime? ToUtc,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResponse<AuditLogResponse>>;
