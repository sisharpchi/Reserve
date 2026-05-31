using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetResources;

public sealed record GetResourcesQuery(Guid TenantId) : IQuery<IReadOnlyList<ResourceResponse>>;
