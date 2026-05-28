using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Resources.Application.Resources.UpdateResource;

public sealed record UpdateResourceCommand(
    Guid ResourceId,
    string Name,
    string ResourceType,
    int Capacity) : ICommand<ResourceResponse?>;
