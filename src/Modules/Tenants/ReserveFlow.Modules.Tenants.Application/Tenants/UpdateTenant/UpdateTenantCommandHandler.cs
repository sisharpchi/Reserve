using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.UpdateTenant;

public sealed class UpdateTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantCommand, TenantResponse?>
{
    public async Task<TenantResponse?> Handle(
        UpdateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        Tenant? tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        string normalizedSlug = Tenant.NormalizeSlug(command.Slug);
        Tenant? tenantWithSlug = await tenantRepository.GetBySlugAsync(normalizedSlug, cancellationToken);

        if (tenantWithSlug is not null && tenantWithSlug.Id != tenant.Id)
        {
            throw new InvalidOperationException("Tenant slug already exists.");
        }

        tenant.Update(
            command.Name,
            normalizedSlug,
            command.TimeZoneId,
            command.CategoryId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }
}
