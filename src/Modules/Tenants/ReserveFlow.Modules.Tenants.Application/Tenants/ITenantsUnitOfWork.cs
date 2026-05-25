namespace ReserveFlow.Modules.Tenants.Application.Tenants;

public interface ITenantsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
