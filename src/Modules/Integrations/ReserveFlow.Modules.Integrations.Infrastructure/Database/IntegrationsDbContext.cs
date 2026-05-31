using Microsoft.EntityFrameworkCore;
using ReserveFlow.Modules.Integrations.Application.WebhookInbox;
using ReserveFlow.Modules.Integrations.Domain.WebhookInbox;
using ReserveFlow.Modules.Integrations.Infrastructure.WebhookInbox;

namespace ReserveFlow.Modules.Integrations.Infrastructure.Database;

public sealed class IntegrationsDbContext(DbContextOptions<IntegrationsDbContext> options)
    : DbContext(options), IIntegrationsUnitOfWork
{
    public DbSet<WebhookInboxMessage> WebhookInboxMessages => Set<WebhookInboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Integrations);
        modelBuilder.ApplyConfiguration(new WebhookInboxMessageConfiguration());
    }
}
