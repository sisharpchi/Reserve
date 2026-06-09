namespace ReserveFlow.Modules.Reporting.Application.Reports;

public interface IStaffUtilizationReportRepository
{
    Task<StaffUtilizationReportResponse> GetAsync(
        Guid tenantId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
