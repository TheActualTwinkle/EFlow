using EFlow.Contracts.Messages.Models;

namespace EFlow.Contracts.Messages;

public record SubmissionSlotCreatedMessage
{
    public required SubmissionSlotModel SubmissionSlot { get; init; }
}