using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Rules;

public sealed class TeacherMiddleNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;

    private readonly string _middleName;

    internal TeacherMiddleNameMustBeProperLengthRule(string middleName) =>
        _middleName = middleName;

    public string Message =>
        $"Teacher middle name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    public bool IsBroken() =>
        _middleName.Length is < MinLengthIncluded or > MaxLengthIncluded;
}