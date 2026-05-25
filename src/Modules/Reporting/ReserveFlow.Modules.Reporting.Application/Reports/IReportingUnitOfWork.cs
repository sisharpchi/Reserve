namespace ReserveFlow.Modules.Reporting.Application.Reports;

public interface IReportingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
