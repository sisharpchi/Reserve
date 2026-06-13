using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.GetNotifications;

namespace ReserveFlow.Modules.Notifications.Presentation;

internal sealed class GetNotificationsEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/notifications", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Notifications")
            .WithName("GetNotifications");
    }

    private static async Task<IResult> Handle(
        ITenantContext tenantContext,
        int? pageNumber,
        int? pageSize,
        string? status,
        string? channel,
        string? search,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetNotificationsQuery, PagedResponse<NotificationResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        PagedResponse<NotificationResponse> response = await handler.Handle(
            new GetNotificationsQuery(
                tenantId,
                pageNumber,
                pageSize,
                status,
                channel,
                search,
                sortBy,
                sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
