namespace EFlow.Common.Domain;

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; }

    public DomainEvent() =>
        Id = Guid.NewGuid();
}