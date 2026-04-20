using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Rules;

public sealed class StudentBirthDateMustBeInPastRule : IBusinessRule
{
    private readonly DateOnly _birthDate;
    private readonly DateOnly _utcNow;
    
    internal StudentBirthDateMustBeInPastRule(DateOnly birthDate, DateOnly utcNow)
    {
        _birthDate = birthDate;
        _utcNow = utcNow;
    }
    
    public string Message =>
        "Student birth date must be in the past.";

    public bool IsBroken() =>
        _birthDate >= _utcNow;
}