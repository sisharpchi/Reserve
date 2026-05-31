using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResource;

public sealed record GetResourceQuery(Guid ResourceId) : IQuery<ResourceResponse?>;
