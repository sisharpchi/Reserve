using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetNoShowReport;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class GetNoShowReportEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/reports/no-show", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Reporting")
            .WithName("GetNoShowReport");
    }

    private static async Task<IResult> Handle(
        DateOnly date,
        ITenantContext tenantContext,
        IQueryHandler<GetNoShowReportQuery, NoShowReportResponse?> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        NoShowReportResponse? response = await handler.Handle(
            new GetNoShowReportQuery(tenantId, date),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
