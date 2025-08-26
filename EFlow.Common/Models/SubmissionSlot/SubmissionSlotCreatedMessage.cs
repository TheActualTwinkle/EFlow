using EFlow.Common.Models.Markers;
using MemoryPack;

namespace EFlow.Common.Models.SubmissionSlot;

[MemoryPackable]
public partial record SubmissionSlotCreatedMessage : IKafkaMessage
{
    public required SubmissionSlotModel SubmissionSlot { get; init; }
}