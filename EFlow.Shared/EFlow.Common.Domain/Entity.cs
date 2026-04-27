using EFlow.Common.Domain.Exceptions;

namespace EFlow.Common.Domain;

public abstract class Entity
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents.AsReadOnly();
    
    private readonly List<IDomainEvent> _domainEvents = [];
    
    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents()
    {
        if (_domainEvents.Count == 0)
            return [];

        var domainEvents = _domainEvents.ToArray();

        _domainEvents.Clear();

        return domainEvents;
    }

    protected static void ThrowIfBroken(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}