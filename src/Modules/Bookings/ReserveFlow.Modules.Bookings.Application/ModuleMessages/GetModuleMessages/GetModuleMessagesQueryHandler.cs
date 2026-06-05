using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;

public sealed class GetModuleMessagesQueryHandler(IModuleMessageRepository moduleMessageRepository)
    : IQueryHandler<GetModuleMessagesQuery, IReadOnlyList<ModuleMessageResponse>>
{
    public Task<IReadOnlyList<ModuleMessageResponse>> Handle(
        GetModuleMessagesQuery query,
        CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(query.Take, 1, 200);

        return moduleMessageRepository.GetByTenantIdAsync(query.TenantId, take, cancellationToken);
    }
}
