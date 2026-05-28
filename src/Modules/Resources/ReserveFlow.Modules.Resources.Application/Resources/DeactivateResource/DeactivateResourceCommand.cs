using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Resources.Application.Resources.DeactivateResource;

public sealed record DeactivateResourceCommand(Guid ResourceId) : ICommand<ResourceResponse?>;
