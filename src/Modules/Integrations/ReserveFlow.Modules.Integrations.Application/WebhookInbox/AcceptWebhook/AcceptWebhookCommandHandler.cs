using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Application.WebhookInbox.AcceptWebhook;

public sealed class AcceptWebhookCommandHandler(
    IWebhookInboxRepository webhookInboxRepository,
    IIntegrationsUnitOfWork unitOfWork) : ICommandHandler<AcceptWebhookCommand, WebhookInboxMessageResponse>
{
    public async Task<WebhookInboxMessageResponse> Handle(
        AcceptWebhookCommand command,
        CancellationToken cancellationToken = default)
    {
        string normalizedSource = WebhookInboxMessage.NormalizeSource(command.Source);
        string normalizedExternalMessageId = WebhookInboxMessage.NormalizeExternalMessageId(command.ExternalMessageId);

        WebhookInboxMessage? existingMessage = await webhookInboxRepository.GetByExternalMessageAsync(
            normalizedSource,
            normalizedExternalMessageId,
            cancellationToken);

        if (existingMessage is not null)
        {
            return WebhookInboxMessageResponse.FromMessage(existingMessage, isDuplicate: true);
        }

        WebhookInboxMessage message = WebhookInboxMessage.Accept(
            command.TenantId,
            normalizedSource,
            normalizedExternalMessageId,
            command.EventType,
            command.PayloadJson);

        webhookInboxRepository.Insert(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return WebhookInboxMessageResponse.FromMessage(message, isDuplicate: false);
    }
}
