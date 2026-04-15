using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Rules;

public sealed class GroupNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;

    private readonly string _name;

    internal GroupNameMustBeProperLengthRule(string name) =>
        _name = name;

    public string Message =>
        $"Group name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    public bool IsBroken() =>
        _name.Length is < MinLengthIncluded or > MaxLengthIncluded;
}