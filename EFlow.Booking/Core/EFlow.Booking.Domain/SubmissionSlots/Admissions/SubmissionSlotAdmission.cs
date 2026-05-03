using EFlow.Booking.Domain.Students;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Admissions;

public sealed class SubmissionSlotAdmission : Entity
{
    public SubmissionSlotAdmissionId Id { get; }

    internal SubmissionSlotId SubmissionSlotId { get; private set; }

    internal StudentId StudentId { get; private set; }
    
    private SubmissionSlotAdmission() { }

    private SubmissionSlotAdmission(
        SubmissionSlotId submissionSlotId,
        StudentId studentId)
    {

        Id = new SubmissionSlotAdmissionId(Guid.CreateVersion7());
        SubmissionSlotId = submissionSlotId;
        StudentId = studentId;
    }

    internal static SubmissionSlotAdmission Create(
        SubmissionSlotId submissionSlotId,
        StudentId studentId) =>
        new(submissionSlotId, studentId);
}
