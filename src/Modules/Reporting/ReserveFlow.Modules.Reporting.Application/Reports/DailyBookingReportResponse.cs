using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports;

public sealed record DailyBookingReportResponse(
    Guid Id,
    Guid TenantId,
    DateOnly Date,
    int CreatedBookings,
    int CancelledBookings,
    int CompletedBookings,
    int NoShowBookings,
    DateTime UpdatedAtUtc)
{
    public static DailyBookingReportResponse FromReport(DailyBookingReport report)
    {
        return new DailyBookingReportResponse(
            report.Id,
            report.TenantId,
            report.Date,
            report.CreatedBookings,
            report.CancelledBookings,
            report.CompletedBookings,
            report.NoShowBookings,
            report.UpdatedAtUtc);
    }
}
