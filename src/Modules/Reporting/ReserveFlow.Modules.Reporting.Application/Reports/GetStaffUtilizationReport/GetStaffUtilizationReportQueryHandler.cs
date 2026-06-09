using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Reporting.Application.Reports.GetStaffUtilizationReport;

public sealed class GetStaffUtilizationReportQueryHandler(
    IStaffUtilizationReportRepository staffUtilizationReportRepository)
    : IQueryHandler<GetStaffUtilizationReportQuery, StaffUtilizationReportResponse>
{
    public Task<StaffUtilizationReportResponse> Handle(
        GetStaffUtilizationReportQuery query,
        CancellationToken cancellationToken = default)
    {
        return staffUtilizationReportRepository.GetAsync(
            query.TenantId,
            query.From,
            query.To,
            cancellationToken);
    }
}
