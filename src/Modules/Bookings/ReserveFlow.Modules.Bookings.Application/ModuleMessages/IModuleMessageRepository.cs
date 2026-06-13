using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages;

public interface IModuleMessageRepository
{
    Task<PagedResult<ModuleMessageResponse>> GetByTenantIdAsync(
        Guid tenantId,
        PageRequest pageRequest,
        string? status,
        string? type,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
