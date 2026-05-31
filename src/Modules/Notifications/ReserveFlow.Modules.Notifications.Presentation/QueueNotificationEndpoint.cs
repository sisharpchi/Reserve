using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Notifications.Application.Notifications;
using ReserveFlow.Modules.Notifications.Application.Notifications.QueueNotification;
using ReserveFlow.Modules.Notifications.Domain.Notifications;

namespace ReserveFlow.Modules.Notifications.Presentation;

internal sealed class QueueNotificationEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/notifications", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Notifications")
            .WithName("QueueNotification");
    }

    private static async Task<IResult> Handle(
        QueueNotificationRequest request,
        ICommandHandler<QueueNotificationCommand, NotificationResponse> handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Channel, ignoreCase: true, out NotificationChannel channel))
        {
            return TypedResults.BadRequest(new { Error = "Unsupported notification channel." });
        }

        NotificationResponse response = await handler.Handle(
            new QueueNotificationCommand(
                request.TenantId,
                channel,
                request.Recipient,
                request.Subject,
                request.Body),
            cancellationToken);

        return TypedResults.Created($"/api/admin/notifications/{response.Id}", response);
    }
}
