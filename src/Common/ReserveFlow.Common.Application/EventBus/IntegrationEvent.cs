namespace ReserveFlow.Common.Application.EventBus;

public abstract record IntegrationEvent(Guid Id, DateTime OccurredOnUtc) : IIntegrationEvent
{
    protected IntegrationEvent()
        : this(Guid.NewGuid(), DateTime.UtcNow)
    {
    }
}
