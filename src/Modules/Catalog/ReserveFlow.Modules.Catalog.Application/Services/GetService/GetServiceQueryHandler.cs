using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetService;

public sealed class GetServiceQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetServiceQuery, ServiceResponse?>
{
    public async Task<ServiceResponse?> Handle(
        GetServiceQuery query,
        CancellationToken cancellationToken = default)
    {
        Service? service = await serviceRepository.GetByIdAsync(query.ServiceId, cancellationToken);

        return service is null ? null : ServiceResponse.FromService(service);
    }
}
