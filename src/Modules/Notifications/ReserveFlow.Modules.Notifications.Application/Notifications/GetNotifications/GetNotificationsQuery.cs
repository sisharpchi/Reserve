using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Status,
    string? Channel,
    string? Search,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResponse<NotificationResponse>>;
