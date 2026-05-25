using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetDailyBookingReport;

public sealed class GetDailyBookingReportQueryHandler(IReportingRepository reportingRepository)
    : IQueryHandler<GetDailyBookingReportQuery, DailyBookingReportResponse?>
{
    public async Task<DailyBookingReportResponse?> Handle(
        GetDailyBookingReportQuery query,
        CancellationToken cancellationToken = default)
    {
        DailyBookingReport? report = await reportingRepository.GetDailyBookingReportAsync(
            query.TenantId,
            query.Date,
            cancellationToken);

        return report is null ? null : DailyBookingReportResponse.FromReport(report);
    }
}
