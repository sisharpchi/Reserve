using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;

namespace ReserveFlow.Modules.Integrations.Presentation;

internal sealed class AcceptWebhookEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/integrations/webhooks/{source}/{externalMessageId}", Handle)
            .WithTags("Integrations")
            .WithName("AcceptWebhook");
    }

    private static async Task<IResult> Handle(
        string source,
        string externalMessageId,
        AcceptWebhookRequest request,
        ICommandHandler<AcceptWebhookCommand, WebhookInboxMessageResponse> handler,
        CancellationToken cancellationToken)
    {
        WebhookInboxMessageResponse response = await handler.Handle(
            new AcceptWebhookCommand(
                request.TenantId,
                source,
                externalMessageId,
                request.EventType,
                request.PayloadJson),
            cancellationToken);

        return TypedResults.Accepted($"/api/integrations/webhooks/{response.Id}", response);
    }
}
