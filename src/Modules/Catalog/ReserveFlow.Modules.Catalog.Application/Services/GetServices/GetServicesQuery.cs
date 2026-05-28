using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetServices;

public sealed record GetServicesQuery(Guid TenantId) : IQuery<IReadOnlyList<ServiceResponse>>;
