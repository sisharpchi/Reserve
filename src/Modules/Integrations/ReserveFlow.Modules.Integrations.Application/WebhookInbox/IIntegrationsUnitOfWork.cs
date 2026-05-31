namespace ReserveFlow.Modules.Integrations.Application.WebhookInbox;

public interface IIntegrationsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
