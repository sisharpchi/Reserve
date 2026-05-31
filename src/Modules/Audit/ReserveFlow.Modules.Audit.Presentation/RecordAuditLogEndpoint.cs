using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Audit.Application.AuditLogs;
using ReserveFlow.Modules.Audit.Application.AuditLogs.RecordAuditLog;

namespace ReserveFlow.Modules.Audit.Presentation;

internal sealed class RecordAuditLogEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/audit-logs", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Audit")
            .WithName("RecordAuditLog");
    }

    private static async Task<IResult> Handle(
        RecordAuditLogRequest request,
        ICommandHandler<RecordAuditLogCommand, AuditLogResponse> handler,
        CancellationToken cancellationToken)
    {
        AuditLogResponse response = await handler.Handle(
            new RecordAuditLogCommand(
                request.TenantId,
                request.UserId,
                request.Action,
                request.EntityName,
                request.EntityId,
                request.DetailsJson),
            cancellationToken);

        return TypedResults.Created($"/api/admin/audit-logs/{response.Id}", response);
    }
}
