using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Rules;

public sealed class StudentFirstNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;

    private readonly string _firstName;

    internal StudentFirstNameMustBeProperLengthRule(string firstName) =>
        _firstName = firstName;

    public string Message =>
        $"Student first name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    public bool IsBroken() =>
        _firstName.Length is < MinLengthIncluded or > MaxLengthIncluded;
}