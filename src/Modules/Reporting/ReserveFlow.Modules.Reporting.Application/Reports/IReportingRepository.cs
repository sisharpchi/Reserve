using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports;

public interface IReportingRepository
{
    void Insert(DailyBookingReport report);

    Task<DailyBookingReport?> GetDailyBookingReportAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default);
}
