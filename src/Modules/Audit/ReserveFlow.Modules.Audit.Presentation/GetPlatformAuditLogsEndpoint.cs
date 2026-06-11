using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Application.AuditLogs.GetPlatformAuditLogs;

namespace ReserveFlow.Modules.Audit.Presentation;

internal sealed class GetPlatformAuditLogsEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/audit-logs", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Platform Audit")
            .WithName("GetPlatformAuditLogs");
    }

    private static async Task<Ok<PagedResponse<AuditLogResponse>>> Handle(
        int? pageNumber,
        int? pageSize,
        Guid? tenantId,
        string? action,
        string? entityName,
        DateTime? fromUtc,
        DateTime? toUtc,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetPlatformAuditLogsQuery, PagedResponse<AuditLogResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<AuditLogResponse> response = await handler.Handle(
            new GetPlatformAuditLogsQuery(
                pageNumber,
                pageSize,
                tenantId,
                action,
                entityName,
                fromUtc,
                toUtc,
                sortBy,
                sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
