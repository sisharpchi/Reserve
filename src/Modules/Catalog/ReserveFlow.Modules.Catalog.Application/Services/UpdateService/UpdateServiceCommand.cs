using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Catalog.Application.Services.UpdateService;

public sealed record UpdateServiceCommand(
    Guid ServiceId,
    string Name,
    int DurationMinutes,
    decimal? Price,
    string? Currency) : ICommand<ServiceResponse?>;
