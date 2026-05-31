using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetService;

public sealed class GetServiceQueryHandler(
    IServiceRepository serviceRepository,
    ITenantAccessGuard tenantAccessGuard)
    : IQueryHandler<GetServiceQuery, ServiceResponse?>
{
    public async Task<ServiceResponse?> Handle(
        GetServiceQuery query,
        CancellationToken cancellationToken = default)
    {
        Service? service = await serviceRepository.GetByIdAsync(query.ServiceId, cancellationToken);

        if (service is null || !tenantAccessGuard.CanAccessTenant(service.TenantId))
        {
            return null;
        }

        return ServiceResponse.FromService(service);
    }
}
