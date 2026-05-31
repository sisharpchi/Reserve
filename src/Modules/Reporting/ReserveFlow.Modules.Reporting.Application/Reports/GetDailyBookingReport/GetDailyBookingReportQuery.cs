using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;

public sealed record GetDailyBookingReportQuery(
    Guid TenantId,
    DateOnly Date) : IQuery<DailyBookingReportResponse?>;
