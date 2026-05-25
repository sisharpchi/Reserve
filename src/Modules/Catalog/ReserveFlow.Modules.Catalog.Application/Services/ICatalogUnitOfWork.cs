namespace ReserveFlow.Modules.Catalog.Application.Services;

public interface ICatalogUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
