using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>
{
    public async Task<IReadOnlyList<NotificationResponse>> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NotificationMessage> messages = await notificationRepository.GetByTenantIdAsync(
            query.TenantId,
            cancellationToken);

        return messages
            .Select(NotificationResponse.FromMessage)
            .ToArray();
    }
}
