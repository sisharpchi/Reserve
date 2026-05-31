using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetActiveServices;

public sealed class GetActiveServicesQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetActiveServicesQuery, IReadOnlyList<ServiceResponse>>
{
    public async Task<IReadOnlyList<ServiceResponse>> Handle(
        GetActiveServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Service> services = await serviceRepository.GetActiveByTenantIdAsync(
            query.TenantId,
            cancellationToken);

        return services
            .Select(ServiceResponse.FromService)
            .ToArray();
    }
}
