using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.DeactivateService;

public sealed class DeactivateServiceCommandHandler(
    IServiceRepository serviceRepository,
    ITenantAccessGuard tenantAccessGuard,
    ICatalogUnitOfWork unitOfWork) : ICommandHandler<DeactivateServiceCommand, ServiceResponse?>
{
    public async Task<ServiceResponse?> Handle(
        DeactivateServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        Service? service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);

        if (service is null || !tenantAccessGuard.CanAccessTenant(service.TenantId))
        {
            return null;
        }

        service.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResponse.FromService(service);
    }
}
