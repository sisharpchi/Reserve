using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Reporting.Application.Reports;
using ReserveFlow.Modules.Reporting.Domain.Reports;
using ReserveFlow.Modules.Reporting.Infrastructure.Database;

namespace ReserveFlow.Modules.Reporting.Infrastructure.Reports;

internal sealed class ReportingRepository(ReportingDbContext dbContext) : IReportingRepository
{
    public void Insert(DailyBookingReport report)
    {
        dbContext.DailyBookingReports.Add(report);
    }

    public async Task<DailyBookingReport?> GetDailyBookingReportAsync(
        Guid tenantId,
        DateOnly reportDate,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.DailyBookingReports
            .FirstOrDefaultAsync(
                report => report.TenantId == tenantId && report.Date == reportDate,
                cancellationToken);
    }
}
