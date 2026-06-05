using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;

public sealed record GetModuleMessagesQuery(
    Guid TenantId,
    int Take = 100) : IQuery<IReadOnlyList<ModuleMessageResponse>>;
