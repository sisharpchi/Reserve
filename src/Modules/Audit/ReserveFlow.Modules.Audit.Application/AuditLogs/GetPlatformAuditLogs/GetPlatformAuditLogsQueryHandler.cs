using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Audit.Domain.AuditLogs;

namespace ReserveFlow.Modules.Audit.Application.AuditLogs.GetPlatformAuditLogs;

public sealed class GetPlatformAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IQueryHandler<GetPlatformAuditLogsQuery, PagedResponse<AuditLogResponse>>
{
    public async Task<PagedResponse<AuditLogResponse>> Handle(
        GetPlatformAuditLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<AuditLog> logs = await auditLogRepository.GetRecentAsync(
            pageRequest,
            query.TenantId,
            query.Action,
            query.EntityName,
            query.FromUtc,
            query.ToUtc,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        AuditLogResponse[] items = logs.Items
            .Select(AuditLogResponse.FromAuditLog)
            .ToArray();

        return new PagedResponse<AuditLogResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            logs.TotalCount);
    }
}
