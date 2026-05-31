using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Presentation.Endpoints;

namespace ReserveFlow.Modules.Notifications.Presentation;

internal sealed class NotificationsStatusEndpoint : IEndpoint
{
    private static readonly string[] Channels =
    [
        "Email",
        "Sms",
        "Telegram",
        "Webhook",
        "InApp"
    ];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications/status", () =>
            Results.Ok(new
            {
                Module = "Notifications",
                Channels,
                Schema = "notifications",
                Sender = "FakeNotificationSender",
                Status = "Configured with queue notification slice, EF Core DbContext, and fake sender baseline"
            }))
            .WithTags("Notifications");
    }
}
