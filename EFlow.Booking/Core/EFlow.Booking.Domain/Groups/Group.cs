using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Subjects;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Students;

namespace EFlow.Booking.Domain.Groups;

public sealed class Group : Entity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public ICollection<Student>? Students { get; init; }

    public ICollection<Subject>? Subjects { get; init; }

    public ICollection<SubmissionSlot>? SubmissionSlots { get; init; }
}