using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Application.TenantCategories;
using ReserveFlow.Modules.Tenants.Domain.TenantCategories;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformUsage;

public sealed class GetPlatformUsageQueryHandler(
    ITenantRepository tenantRepository,
    ITenantCategoryRepository categoryRepository) : IQueryHandler<GetPlatformUsageQuery, PlatformUsageResponse>
{
    public async Task<PlatformUsageResponse> Handle(
        GetPlatformUsageQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Tenant> tenants = await tenantRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<TenantCategory> categories = await categoryRepository.GetAllAsync(cancellationToken);

        return new PlatformUsageResponse(
            tenants.Count,
            tenants.Count(tenant => tenant.Status is TenantStatus.Pending),
            tenants.Count(tenant => tenant.Status is TenantStatus.Active),
            tenants.Count(tenant => tenant.Status is TenantStatus.Suspended),
            tenants.Count(tenant => tenant.Status is TenantStatus.Deleted),
            categories.Count,
            categories.Count(category => category.IsActive),
            DateTimeOffset.UtcNow);
    }
}
