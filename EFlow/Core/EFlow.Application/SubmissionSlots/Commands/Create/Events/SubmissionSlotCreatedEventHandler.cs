using MediatR;
using Microsoft.Extensions.Logging;

namespace EFlow.Application.SubmissionSlots.Commands.Events;

public class SubmissionSlotCreatedEventHandler(ILogger<SubmissionSlotCreatedEventHandler> logger) 
    : INotificationHandler<SubmissionSlotCreatedEvent>
{
    public Task Handle(SubmissionSlotCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Submission slot created. ID = {SubmissionSlot}", notification.SubmissionSlot.Id);

        return Task.CompletedTask;
    }
}