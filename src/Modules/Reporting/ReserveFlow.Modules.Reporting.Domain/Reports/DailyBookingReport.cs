using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Reporting.Domain.Reports;

public sealed class DailyBookingReport : Entity
{
    private DailyBookingReport(
        Guid id,
        Guid tenantId,
        DateOnly date,
        int createdBookings,
        int cancelledBookings,
        int completedBookings,
        int noShowBookings)
        : base(id)
    {
        TenantId = tenantId;
        Date = date;
        CreatedBookings = createdBookings;
        CancelledBookings = cancelledBookings;
        CompletedBookings = completedBookings;
        NoShowBookings = noShowBookings;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private DailyBookingReport()
    {
    }

    public Guid TenantId { get; private set; }

    public DateOnly Date { get; private set; }

    public int CreatedBookings { get; private set; }

    public int CancelledBookings { get; private set; }

    public int CompletedBookings { get; private set; }

    public int NoShowBookings { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static DailyBookingReport Record(
        Guid tenantId,
        DateOnly date,
        int createdBookings,
        int cancelledBookings,
        int completedBookings,
        int noShowBookings)
    {
        Validate(tenantId, date, createdBookings, cancelledBookings, completedBookings, noShowBookings);

        var report = new DailyBookingReport(
            Guid.NewGuid(),
            tenantId,
            date,
            createdBookings,
            cancelledBookings,
            completedBookings,
            noShowBookings);

        report.RaiseDomainEvent(new DailyBookingReportRecordedDomainEvent(report.Id, tenantId, date));

        return report;
    }

    public void ReplaceCounts(
        int createdBookings,
        int cancelledBookings,
        int completedBookings,
        int noShowBookings)
    {
        Validate(TenantId, Date, createdBookings, cancelledBookings, completedBookings, noShowBookings);

        CreatedBookings = createdBookings;
        CancelledBookings = cancelledBookings;
        CompletedBookings = completedBookings;
        NoShowBookings = noShowBookings;
        UpdatedAtUtc = DateTime.UtcNow;

        RaiseDomainEvent(new DailyBookingReportRecordedDomainEvent(Id, TenantId, Date));
    }

    private static void Validate(
        Guid tenantId,
        DateOnly date,
        int createdBookings,
        int cancelledBookings,
        int completedBookings,
        int noShowBookings)
    {
        if (tenantId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is required.");
        }

        if (date == default)
        {
            throw new InvalidOperationException("Report date is required.");
        }

        if (createdBookings < 0 || cancelledBookings < 0 || completedBookings < 0 || noShowBookings < 0)
        {
            throw new InvalidOperationException("Report booking counts cannot be negative.");
        }
    }
}
