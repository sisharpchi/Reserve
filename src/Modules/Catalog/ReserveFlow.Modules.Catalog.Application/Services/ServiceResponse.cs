using ReserveFlow.Modules.Catalog.Domain.Services;

namespace ReserveFlow.Modules.Catalog.Application.Services;

public sealed record ServiceResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    int DurationMinutes,
    decimal? Price,
    string? Currency,
    bool IsActive)
{
    public static ServiceResponse FromService(Service service)
    {
        return new ServiceResponse(
            service.Id,
            service.TenantId,
            service.Name,
            service.DurationMinutes,
            service.Price,
            service.Currency,
            service.IsActive);
    }
}
