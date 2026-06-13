using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetActiveService;

public sealed record GetActiveServiceQuery(
    Guid TenantId,
    Guid ServiceId) : IQuery<ServiceResponse?>;
