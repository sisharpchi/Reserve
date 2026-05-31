namespace ReserveFlow.Modules.Scheduling.Application.WorkingHours;

public interface ISchedulingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
