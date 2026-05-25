using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Application.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetActiveServices;

public sealed record GetActiveServicesQuery(Guid TenantId) : IQuery<IReadOnlyList<ServiceResponse>>;
