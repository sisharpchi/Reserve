using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
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

    private static async Task<Ok<IReadOnlyList<AuditLogResponse>>> Handle(
        int? limit,
        IQueryHandler<GetPlatformAuditLogsQuery, IReadOnlyList<AuditLogResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AuditLogResponse> response = await handler.Handle(
            new GetPlatformAuditLogsQuery(limit ?? 100),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
