using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.UpdateService;

public sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository,
    ICatalogUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceCommand, ServiceResponse?>
{
    public async Task<ServiceResponse?> Handle(
        UpdateServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        Service? service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);

        if (service is null)
        {
            return null;
        }

        service.Update(
            command.Name,
            command.DurationMinutes,
            command.Price,
            command.Currency);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResponse.FromService(service);
    }
}
