namespace EFlow.Common.Domain;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }

    public DateTime OccurredOn { get; }

    public DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow; // TODO: Use SystemClock
    }
}