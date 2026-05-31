using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPublicTenants;

public sealed class GetPublicTenantsQueryHandler(ITenantRepository tenantRepository)
    : IQueryHandler<GetPublicTenantsQuery, IReadOnlyList<TenantResponse>>
{
    public async Task<IReadOnlyList<TenantResponse>> Handle(
        GetPublicTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Tenant> tenants = await tenantRepository.GetPublicAsync(
            query.CategoryId,
            query.Search,
            cancellationToken);

        return tenants
            .Select(TenantResponse.FromTenant)
            .ToArray();
    }
}
