using EFlow.Common.Domain.Exceptions;

namespace EFlow.Common.Domain;

public abstract class Entity
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents => 
        _domainEvents.AsReadOnly();
    
    private readonly List<IDomainEvent> _domainEvents = [];
    
    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    protected void ThrowIfRuleBroken(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}