using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformTenants;

public sealed record GetPlatformTenantsQuery : IQuery<IReadOnlyList<TenantResponse>>;
