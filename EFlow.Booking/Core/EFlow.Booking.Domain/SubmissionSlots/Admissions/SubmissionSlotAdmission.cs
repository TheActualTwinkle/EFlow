using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Admissions;

public sealed class SubmissionSlotAdmission : Entity
{
    public SubmissionSlotAdmissionId Id { get; }

    internal SubmissionSlotId SubmissionSlotId { get; private set; }

    internal StudentId StudentId { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    private SubmissionSlotAdmission() { }

    private SubmissionSlotAdmission(
        SubmissionSlotId submissionSlotId,
        StudentId studentId,
        DateTime createdAt,
        DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(createdAt, utcNow));

        Id = new SubmissionSlotAdmissionId(Guid.CreateVersion7());
        SubmissionSlotId = submissionSlotId;
        StudentId = studentId;
        CreatedAt = createdAt;
    }

    internal static SubmissionSlotAdmission Create(
        SubmissionSlotId submissionSlotId,
        StudentId studentId,
        DateTime createdAt,
        DateTime utcNow) =>
        new(submissionSlotId, studentId, createdAt, utcNow);
}
