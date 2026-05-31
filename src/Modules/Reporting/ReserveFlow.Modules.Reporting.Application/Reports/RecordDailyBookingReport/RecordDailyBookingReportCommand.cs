using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Reporting.Application.Reports.RecordDailyBookingReport;

public sealed record RecordDailyBookingReportCommand(
    Guid TenantId,
    DateOnly Date,
    int CreatedBookings,
    int CancelledBookings,
    int CompletedBookings,
    int NoShowBookings) : ICommand<DailyBookingReportResponse>;
