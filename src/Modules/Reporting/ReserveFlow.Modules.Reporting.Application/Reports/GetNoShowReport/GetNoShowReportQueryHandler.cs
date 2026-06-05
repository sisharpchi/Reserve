using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Reporting.Domain.Reports;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetNoShowReport;

public sealed class GetNoShowReportQueryHandler(IReportingRepository reportingRepository)
    : IQueryHandler<GetNoShowReportQuery, NoShowReportResponse?>
{
    public async Task<NoShowReportResponse?> Handle(
        GetNoShowReportQuery query,
        CancellationToken cancellationToken = default)
    {
        DailyBookingReport? report = await reportingRepository.GetDailyBookingReportAsync(
            query.TenantId,
            query.Date,
            cancellationToken);

        return report is null ? null : NoShowReportResponse.FromReport(report);
    }
}
