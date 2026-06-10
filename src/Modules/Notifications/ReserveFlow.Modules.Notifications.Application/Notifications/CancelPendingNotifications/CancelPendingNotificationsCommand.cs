using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.CancelPendingNotifications;

public sealed record CancelPendingNotificationsCommand(
    Guid TenantId,
    string CorrelationKey,
    string Reason) : ICommand<int>;
