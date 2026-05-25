namespace ReserveFlow.Common.Domain;

public abstract record DomainEvent(Guid Id, DateTime OccurredOnUtc) : IDomainEvent
{
    protected DomainEvent()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }
}
