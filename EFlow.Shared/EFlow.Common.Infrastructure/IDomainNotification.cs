using MediatR;

namespace EFlow.Common.Infrastructure;

public interface IDomainNotification<TDomainEvent> : INotification
{
    TDomainEvent DomainEvent { get; init; }
}
