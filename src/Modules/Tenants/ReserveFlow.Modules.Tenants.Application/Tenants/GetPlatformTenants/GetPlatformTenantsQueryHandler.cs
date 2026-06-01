using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

public sealed class GetPlatformTenantsQueryHandler(ITenantRepository tenantRepository)
    : IQueryHandler<GetPlatformTenantsQuery, IReadOnlyList<TenantResponse>>
{
    public async Task<IReadOnlyList<TenantResponse>> Handle(
        GetPlatformTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        var tenants = await tenantRepository.GetAllAsync(cancellationToken);

        return tenants
            .Select(TenantResponse.FromTenant)
            .ToArray();
    }
}
