using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.ActivateTenant;

public sealed class ActivateTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<ActivateTenantCommand, TenantResponse?>
{
    public async Task<TenantResponse?> Handle(
        ActivateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        Tenant? tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        tenant.Activate(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }
}
