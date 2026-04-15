using EFlow.Booking.Subjects;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Rules;

public sealed class SubjectMustNotBeAddedTwiceRule : IBusinessRule
{
    private readonly IEnumerable<Subject> ExistingSubjects;
    private readonly Subject AddedSubject;

    internal SubjectMustNotBeAddedTwiceRule(IEnumerable<Subject> existingSubjects, Subject addedSubject)
    {
        ExistingSubjects = existingSubjects;
        AddedSubject = addedSubject;
    }

    public string Message =>
        "Subject must not be added to the group twice.";

    public bool IsBroken() =>
        ExistingSubjects.Any(s => s.Id == AddedSubject.Id);
}