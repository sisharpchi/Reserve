using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.GetStaffUtilizationReport;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class GetStaffUtilizationReportEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/reports/staff-utilization", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Reporting")
            .WithName("GetStaffUtilizationReport");
    }

    private static async Task<IResult> Handle(
        DateOnly? from,
        DateOnly? to,
        ITenantContext tenantContext,
        IQueryHandler<GetStaffUtilizationReportQuery, StaffUtilizationReportResponse> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        DateOnly toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly fromDate = from ?? toDate.AddDays(-30);

        if (toDate < fromDate)
        {
            return TypedResults.BadRequest("Report end date must be on or after start date.");
        }

        StaffUtilizationReportResponse response = await handler.Handle(
            new GetStaffUtilizationReportQuery(tenantId, fromDate, toDate),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
