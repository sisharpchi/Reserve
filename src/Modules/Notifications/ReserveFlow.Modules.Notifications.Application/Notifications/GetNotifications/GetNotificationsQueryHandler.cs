using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    : IQueryHandler<GetNotificationsQuery, PagedResponse<NotificationResponse>>
{
    public async Task<PagedResponse<NotificationResponse>> Handle(
        GetNotificationsQuery query,
        CancellationToken cancellationToken = default)
    {
        PageRequest pageRequest = PageRequest.Create(query.PageNumber, query.PageSize);
        PagedResult<NotificationMessage> messages = await notificationRepository.GetByTenantIdAsync(
            query.TenantId,
            pageRequest,
            query.Status,
            query.Channel,
            query.Search,
            query.SortBy,
            query.SortDirection,
            cancellationToken);

        NotificationResponse[] items = messages.Items
            .Select(NotificationResponse.FromMessage)
            .ToArray();

        return new PagedResponse<NotificationResponse>(
            items,
            pageRequest.PageNumber,
            pageRequest.PageSize,
            messages.TotalCount);
    }
}
