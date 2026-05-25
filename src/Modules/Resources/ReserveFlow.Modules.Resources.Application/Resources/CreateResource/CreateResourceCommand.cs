using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Application.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.CreateResource;

public sealed record CreateResourceCommand(
    Guid TenantId,
    string Name,
    string ResourceType,
    int Capacity) : ICommand<ResourceResponse>;
