using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class TeacherMustOwnSubjectRule : IBusinessRule
{
    private readonly TeacherId _teacherId;
    private readonly TeacherId _subjectTeacherId;

    internal TeacherMustOwnSubjectRule(TeacherId teacherId, TeacherId subjectTeacherId)
    {
        _teacherId = teacherId;
        _subjectTeacherId = subjectTeacherId;
    }

    public string Message => "Teacher must own subject.";

    public bool IsBroken() =>
        _teacherId != _subjectTeacherId;
}
