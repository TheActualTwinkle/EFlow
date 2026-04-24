using EFlow.Common.Markers;
using MemoryPack;

namespace EFlow.Booking.IntegrationEvents.SubmissionSlots;

[MemoryPackable]
public partial record SubmissionSlotCreatedIntegrationEvent : IKafkaMessage
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
}
