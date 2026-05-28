using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.DeactivateService;

public sealed record DeactivateServiceCommand(Guid ServiceId) : ICommand<ServiceResponse?>;
