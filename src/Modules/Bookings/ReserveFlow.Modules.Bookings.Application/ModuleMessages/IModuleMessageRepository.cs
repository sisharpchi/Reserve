namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages;

public interface IModuleMessageRepository
{
    Task<IReadOnlyList<ModuleMessageResponse>> GetByTenantIdAsync(
        Guid tenantId,
        int take,
        CancellationToken cancellationToken = default);
}
