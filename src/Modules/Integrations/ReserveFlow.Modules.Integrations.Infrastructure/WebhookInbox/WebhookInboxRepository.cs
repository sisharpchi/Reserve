using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;
using ReserveFlow.Modules.Integrations.Infrastructure.Database;

namespace ReserveFlow.Modules.Integrations.Infrastructure.WebhookInbox;

internal sealed class WebhookInboxRepository(IntegrationsDbContext dbContext) : IWebhookInboxRepository
{
    public void Insert(WebhookInboxMessage message)
    {
        dbContext.WebhookInboxMessages.Add(message);
    }

    public async Task<WebhookInboxMessage?> GetByExternalMessageAsync(
        string source,
        string externalMessageId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WebhookInboxMessages
            .FirstOrDefaultAsync(
                message => message.Source == source && message.ExternalMessageId == externalMessageId,
                cancellationToken);
    }
}
