using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Tenants.Domain.Tenants;

namespace ReserveFlow.Modules.Tenants.Application.Tenants.SuspendTenant;

public sealed class SuspendTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantsUnitOfWork unitOfWork) : ICommandHandler<SuspendTenantCommand, TenantResponse?>
{
    public async Task<TenantResponse?> Handle(
        SuspendTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        Tenant? tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        tenant.Suspend(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return TenantResponse.FromTenant(tenant);
    }
}
