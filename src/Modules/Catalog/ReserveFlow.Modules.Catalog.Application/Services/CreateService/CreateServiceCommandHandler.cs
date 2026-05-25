using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.CreateService;

public sealed class CreateServiceCommandHandler(
    IServiceRepository serviceRepository,
    ICatalogUnitOfWork unitOfWork) : ICommandHandler<CreateServiceCommand, ServiceResponse>
{
    public async Task<ServiceResponse> Handle(
        CreateServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        Service service = Service.Create(
            command.TenantId,
            command.Name,
            command.DurationMinutes,
            command.Price,
            command.Currency);

        serviceRepository.Insert(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResponse.FromService(service);
    }
}
