using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.UpdateResource;

public sealed class UpdateResourceCommandHandler(
    IResourceRepository resourceRepository,
    IResourcesUnitOfWork unitOfWork) : ICommandHandler<UpdateResourceCommand, ResourceResponse?>
{
    public async Task<ResourceResponse?> Handle(
        UpdateResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        Resource? resource = await resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);

        if (resource is null)
        {
            return null;
        }

        resource.Update(
            command.Name,
            command.ResourceType,
            command.Capacity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResourceResponse.FromResource(resource);
    }
}
