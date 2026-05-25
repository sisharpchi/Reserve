using Microsoft.EntityFrameworkCore;
using ReserveFlow.Common.Domain;
using ReserveFlow.Common.Infrastructure.Outbox;
using ReserveFlow.Modules.Bookings.Domain.BookingPolicies;
using ReserveFlow.Modules.Bookings.Domain.BookingHistory;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Domain.Bookings;
using ReserveFlow.Modules.Bookings.Infrastructure.BookingPolicies;
using ReserveFlow.Modules.Bookings.Infrastructure.BookingHistory;
using ReserveFlow.Modules.Bookings.Infrastructure.Bookings;
using ReserveFlow.Modules.Bookings.Domain.Customers;
using ReserveFlow.Modules.Bookings.Infrastructure.Customers;
using ReserveFlow.Modules.Bookings.Infrastructure.Outbox;

namespace ReserveFlow.Modules.Bookings.Infrastructure.Database;

public sealed class BookingsDbContext(DbContextOptions<BookingsDbContext> options)
    : DbContext(options), IBookingsUnitOfWork
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingPolicy> BookingPolicies => Set<BookingPolicy>();

    public DbSet<BookingHistoryEntry> BookingHistoryEntries => Set<BookingHistoryEntry>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessagesFromDomainEvents();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Bookings);
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new BookingPolicyConfiguration());
        modelBuilder.ApplyConfiguration(new BookingHistoryEntryConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    private void AddOutboxMessagesFromDomainEvents()
    {
        Entity[] entities = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();

        foreach (Entity entity in entities)
        {
            IDomainEvent[] domainEvents = entity.DomainEvents.ToArray();

            entity.ClearDomainEvents();

            foreach (IDomainEvent domainEvent in domainEvents)
            {
                OutboxMessages.Add(OutboxMessage.FromDomainEvent(domainEvent));
            }
        }
    }
}
