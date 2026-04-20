using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Rules;

public sealed class TeacherLastNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;

    private readonly string _lastName;

    internal TeacherLastNameMustBeProperLengthRule(string lastName) =>
        _lastName = lastName;

    public string Message =>
        $"Teacher last name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    public bool IsBroken() =>
        _lastName.Length is < MinLengthIncluded or > MaxLengthIncluded;
}