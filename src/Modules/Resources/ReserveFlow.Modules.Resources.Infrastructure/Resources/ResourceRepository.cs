using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Resources.Application.Resources;
using ReserveFlow.Modules.Resources.Domain.Resources;
using ReserveFlow.Modules.Resources.Infrastructure.Database;

namespace ReserveFlow.Modules.Resources.Infrastructure.Resources;

internal sealed class ResourceRepository(ResourcesDbContext dbContext) : IResourceRepository
{
    public void Insert(Resource resource)
    {
        dbContext.Resources.Add(resource);
    }

    public async Task<Resource?> GetByIdAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources.FirstOrDefaultAsync(
            resource => resource.Id == resourceId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources
            .Where(resource => resource.TenantId == tenantId)
            .OrderBy(resource => resource.Name)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Resource>> GetActiveByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Resources
            .Where(resource => resource.TenantId == tenantId && resource.IsActive)
            .OrderBy(resource => resource.Name)
            .ToArrayAsync(cancellationToken);
    }
}
