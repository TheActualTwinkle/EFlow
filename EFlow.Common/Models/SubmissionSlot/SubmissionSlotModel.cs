using MemoryPack;

namespace EFlow.Common.Models.SubmissionSlot;

[MemoryPackable]
public partial record SubmissionSlotModel
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
}