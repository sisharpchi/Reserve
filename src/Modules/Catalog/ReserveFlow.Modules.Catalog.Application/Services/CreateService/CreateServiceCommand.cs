using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Catalog.Application.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services.CreateService;

public sealed record CreateServiceCommand(
    Guid TenantId,
    string Name,
    int DurationMinutes,
    decimal? Price,
    string? Currency) : ICommand<ServiceResponse>;
