using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Domain.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.CreateResource;

public sealed class CreateResourceCommandHandler(
    IResourceRepository resourceRepository,
    IResourcesUnitOfWork unitOfWork) : ICommandHandler<CreateResourceCommand, ResourceResponse>
{
    public async Task<ResourceResponse> Handle(
        CreateResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        Resource resource = Resource.Create(
            command.TenantId,
            command.Name,
            command.ResourceType,
            command.Capacity);

        resourceRepository.Insert(resource);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ResourceResponse.FromResource(resource);
    }
}
