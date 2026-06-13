using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports;

public sealed record NoShowReportResponse(
    Guid TenantId,
    DateOnly Date,
    int CreatedBookings,
    int CompletedBookings,
    int NoShowBookings,
    int OutcomeBookings,
    decimal NoShowRate)
{
    public static NoShowReportResponse FromReport(DailyBookingReport report)
    {
        int outcomeBookings = report.CompletedBookings + report.NoShowBookings;
        decimal noShowRate = outcomeBookings == 0
            ? 0m
            : Math.Round((decimal)report.NoShowBookings / outcomeBookings, 4);

        return new NoShowReportResponse(
            report.TenantId,
            report.Date,
            report.CreatedBookings,
            report.CompletedBookings,
            report.NoShowBookings,
            outcomeBookings,
            noShowRate);
    }
}
