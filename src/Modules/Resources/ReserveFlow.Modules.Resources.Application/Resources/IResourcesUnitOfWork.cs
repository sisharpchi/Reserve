namespace ReserveFlow.Modules.Resources.Application.Resources;

public interface IResourcesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
