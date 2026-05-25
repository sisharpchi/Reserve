using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Application.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string TimeZoneId) : ICommand<TenantResponse>;
