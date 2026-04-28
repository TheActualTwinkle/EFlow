using EFlow.Booking.Domain.SubmissionSlots.Admissions;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class StudentMustHaveAdmissionToSubmissionSlotRule : IBusinessRule
{
    private readonly SubmissionSlotAdmission? _admission;

    internal StudentMustHaveAdmissionToSubmissionSlotRule(SubmissionSlotAdmission? admission) =>
        _admission = admission;

    public string Message => "Student must have admission to the submission slot.";

    public bool IsBroken() => _admission is null;
}
