using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetActiveService;

public sealed class GetActiveServiceQueryHandler(IServiceRepository serviceRepository)
    : IQueryHandler<GetActiveServiceQuery, ServiceResponse?>
{
    public async Task<ServiceResponse?> Handle(
        GetActiveServiceQuery query,
        CancellationToken cancellationToken = default)
    {
        Service? service = await serviceRepository.GetActiveByIdAsync(
            query.TenantId,
            query.ServiceId,
            cancellationToken);

        return service is null ? null : ServiceResponse.FromService(service);
    }
}
