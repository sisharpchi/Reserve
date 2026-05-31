using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Domain.Services;
using ReserveFlow.Modules.Catalog.Infrastructure.Database;

namespace ReserveFlow.Modules.Catalog.Infrastructure.Services;

internal sealed class ServiceRepository(CatalogDbContext dbContext) : IServiceRepository
{
    public void Insert(Service service)
    {
        dbContext.Services.Add(service);
    }

    public async Task<Service?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Services.FirstOrDefaultAsync(
            service => service.Id == serviceId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Services
            .Where(service => service.TenantId == tenantId)
            .OrderBy(service => service.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Service>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Services
            .Where(service => service.TenantId == tenantId && service.IsActive)
            .OrderBy(service => service.Name)
            .ToArrayAsync(cancellationToken);
    }
}
