using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class GetTenantDailyReportEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/reports/daily", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Reporting")
            .WithName("GetTenantDailyReport");
    }

    private static async Task<IResult> Handle(
        DateOnly date,
        ITenantContext tenantContext,
        IQueryHandler<GetDailyBookingReportQuery, DailyBookingReportResponse?> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        DailyBookingReportResponse? response = await handler.Handle(
            new GetDailyBookingReportQuery(tenantId, date),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
