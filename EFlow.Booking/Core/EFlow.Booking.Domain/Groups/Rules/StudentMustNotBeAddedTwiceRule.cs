using EFlow.Booking.Domain.Students;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Rules;

public sealed class StudentMustNotBeAddedTwiceRule : IBusinessRule
{
    private readonly IEnumerable<Student> ExistingStudents;
    private readonly Student AddedStudent;

    internal StudentMustNotBeAddedTwiceRule(IEnumerable<Student> existingStudents, Student addedStudent)
    {
        ExistingStudents = existingStudents;
        AddedStudent = addedStudent;
    }

    public string Message =>
        "Student must not be added to the group twice.";

    public bool IsBroken() =>
        ExistingStudents.Any(s => s.Id == AddedStudent.Id);
}