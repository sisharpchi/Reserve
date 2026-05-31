using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Resources.Application.Resources;

namespace ReserveFlow.Modules.Resources.Application.Resources.GetActiveResources;

public sealed record GetActiveResourcesQuery(Guid TenantId) : IQuery<IReadOnlyList<ResourceResponse>>;
