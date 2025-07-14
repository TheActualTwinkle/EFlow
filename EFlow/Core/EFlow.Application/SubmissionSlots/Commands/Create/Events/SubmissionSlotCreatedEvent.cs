using MediatR;

namespace EFlow.Application.SubmissionSlots.Commands.Events;

public record SubmissionSlotCreatedEvent : INotification
{
    public required SubmissionSlotDto SubmissionSlot { get; init; }
}