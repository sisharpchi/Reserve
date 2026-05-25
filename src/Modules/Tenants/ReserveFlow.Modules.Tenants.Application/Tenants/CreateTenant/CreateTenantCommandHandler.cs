using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.CreateTenant;

public sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<CreateTenantCommand, TenantResponse>
{
    public async Task<TenantResponse> Handle(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        string normalizedSlug = Tenant.NormalizeSlug(command.Slug);

        if (await tenantRepository.ExistsBySlugAsync(normalizedSlug, cancellationToken))
        {
            throw new InvalidOperationException("Tenant slug already exists.");
        }

        Tenant tenant = Tenant.Create(command.Name, normalizedSlug, command.TimeZoneId);

        tenantRepository.Insert(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }
}
