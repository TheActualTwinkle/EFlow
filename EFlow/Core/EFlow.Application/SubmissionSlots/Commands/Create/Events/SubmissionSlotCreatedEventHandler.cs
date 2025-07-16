using EFlow.Contracts.Messages;
using EFlow.Contracts.Messages.Models;
using Mapster;
using MassTransit;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Commands.Events;

public class SubmissionSlotCreatedEventHandler(IBus bus)
    : INotificationHandler<SubmissionSlotCreatedEvent>
{
    public async Task Handle(SubmissionSlotCreatedEvent notification, CancellationToken cancellationToken)
    {
        var message = new SubmissionSlotCreatedMessage
        {
            SubmissionSlot = notification.SubmissionSlot.Adapt<SubmissionSlotModel>()
        };

        await bus.Publish(message, cancellationToken);
    }
}