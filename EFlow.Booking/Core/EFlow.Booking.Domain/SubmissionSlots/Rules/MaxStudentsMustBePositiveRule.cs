using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class MaxStudentsMustBePositiveRule : IBusinessRule
{
    private readonly int _maxStudents;

    internal MaxStudentsMustBePositiveRule(int maxStudents) =>
        _maxStudents = maxStudents;

    public string Message =>
        $"Submission slot max students must be positive, but was {_maxStudents}.";

    public bool IsBroken() =>
        _maxStudents < 0;
}