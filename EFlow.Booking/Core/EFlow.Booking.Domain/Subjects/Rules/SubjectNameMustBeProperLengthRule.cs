using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects.Rules;

/// <inheritdoc />
public sealed class SubjectNameMustBeProperLengthRule : IBusinessRule
{
    public const int MinLengthIncluded = 1;
    public const int MaxLengthIncluded = 64;
    
    private readonly string _name;

    internal SubjectNameMustBeProperLengthRule(string name) =>
        _name = name;

    /// <inheritdoc />
    public string Message => 
        $"Subject name must be between {MinLengthIncluded} and {MaxLengthIncluded} characters long.";

    /// <inheritdoc />
    public bool IsBroken() =>
        _name.Length is < MinLengthIncluded or > MaxLengthIncluded;
}