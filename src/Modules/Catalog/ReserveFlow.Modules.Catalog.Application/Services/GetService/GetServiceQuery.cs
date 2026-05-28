using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.GetService;

public sealed record GetServiceQuery(Guid ServiceId) : IQuery<ServiceResponse?>;
