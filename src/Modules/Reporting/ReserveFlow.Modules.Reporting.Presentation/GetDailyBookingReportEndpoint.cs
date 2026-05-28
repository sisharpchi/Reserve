using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class GetDailyBookingReportEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/reports/daily-bookings", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Reporting")
            .WithName("GetDailyBookingReport");
    }

    private static async Task<Results<Ok<DailyBookingReportResponse>, NotFound>> Handle(
        Guid tenantId,
        DateOnly date,
        IQueryHandler<GetDailyBookingReportQuery, DailyBookingReportResponse?> handler,
        CancellationToken cancellationToken)
    {
        DailyBookingReportResponse? response = await handler.Handle(
            new GetDailyBookingReportQuery(tenantId, date),
            cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }
}
