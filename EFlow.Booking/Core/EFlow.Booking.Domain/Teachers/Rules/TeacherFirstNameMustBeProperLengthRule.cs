using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Rules;

public sealed class TeacherFirstNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;

    private readonly string _firstName;

    internal TeacherFirstNameMustBeProperLengthRule(string firstName) =>
        _firstName = firstName;

    public string Message =>
        $"Teacher first name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    public bool IsBroken() =>
        _firstName.Length is < MinLengthIncluded or > MaxLengthIncluded;
}