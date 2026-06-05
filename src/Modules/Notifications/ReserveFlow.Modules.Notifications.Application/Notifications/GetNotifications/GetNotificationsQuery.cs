using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(Guid TenantId)
    : IQuery<IReadOnlyList<NotificationResponse>>;
