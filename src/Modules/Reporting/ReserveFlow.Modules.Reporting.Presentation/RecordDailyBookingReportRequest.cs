namespace ReserveFlow.Modules.Reporting.Presentation;

internal sealed record RecordDailyBookingReportRequest(
    Guid TenantId,
    DateOnly Date,
    int CreatedBookings,
    int CancelledBookings,
    int CompletedBookings,
    int NoShowBookings);
