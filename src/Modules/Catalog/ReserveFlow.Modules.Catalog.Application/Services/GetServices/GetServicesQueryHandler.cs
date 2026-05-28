using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetServices;

public sealed class GetServicesQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetServicesQuery, IReadOnlyList<ServiceResponse>>
{
    public async Task<IReadOnlyList<ServiceResponse>> Handle(
        GetServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        var services = await serviceRepository.GetByTenantIdAsync(query.TenantId, cancellationToken);

        return services
            .Select(ServiceResponse.FromService)
            .ToArray();
    }
}
