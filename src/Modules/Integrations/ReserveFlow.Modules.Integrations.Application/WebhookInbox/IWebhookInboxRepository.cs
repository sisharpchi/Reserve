using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Application.WebhookInbox;

public interface IWebhookInboxRepository
{
    void Insert(WebhookInboxMessage message);

    Task<WebhookInboxMessage?> GetByExternalMessageAsync(
        string source,
        string externalMessageId,
        CancellationToken cancellationToken = default);
}
