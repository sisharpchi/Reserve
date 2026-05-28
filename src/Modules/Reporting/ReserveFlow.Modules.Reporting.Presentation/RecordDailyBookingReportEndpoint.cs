using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;

namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed class RecordDailyBookingReportEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/reports/daily-bookings", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Reporting")
            .WithName("RecordDailyBookingReport");
    }

    private static async Task<IResult> Handle(
        RecordDailyBookingReportRequest request,
        ICommandHandler<RecordDailyBookingReportCommand, DailyBookingReportResponse> handler,
        CancellationToken cancellationToken)
    {
        DailyBookingReportResponse response = await handler.Handle(
            new RecordDailyBookingReportCommand(
                request.TenantId,
                request.Date,
                request.CreatedBookings,
                request.CancelledBookings,
                request.CompletedBookings,
                request.NoShowBookings),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
