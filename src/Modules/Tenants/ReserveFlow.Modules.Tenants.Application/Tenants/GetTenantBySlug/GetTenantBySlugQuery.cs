using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Application.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.GetTenantBySlug;

public sealed record GetTenantBySlugQuery(string Slug) : IQuery<TenantResponse?>;
