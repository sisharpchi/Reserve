using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string Slug,
    string TimeZoneId,
    Guid? CategoryId) : ICommand<TenantResponse?>;
