using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Reporting.Domain.Reports;

public sealed record DailyBookingReportRecordedDomainEvent(
    Guid ReportId,
    Guid TenantId,
    DateOnly Date) : DomainEvent;
