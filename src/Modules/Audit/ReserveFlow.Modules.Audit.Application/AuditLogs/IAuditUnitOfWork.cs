namespace ReserveFlow.Modules.Audit.Application.AuditLogs;

public interface IAuditUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
