using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.DeactivateResource;

public sealed class DeactivateResourceCommandHandler(
    IResourceRepository resourceRepository,
    IResourcesUnitOfWork unitOfWork) : ICommandHandler<DeactivateResourceCommand, ResourceResponse?>
{
    public async Task<ResourceResponse?> Handle(
        DeactivateResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        Resource? resource = await resourceRepository.GetByIdAsync(command.ResourceId, cancellationToken);

        if (resource is null)
        {
            return null;
        }

        resource.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResourceResponse.FromResource(resource);
    }
}
