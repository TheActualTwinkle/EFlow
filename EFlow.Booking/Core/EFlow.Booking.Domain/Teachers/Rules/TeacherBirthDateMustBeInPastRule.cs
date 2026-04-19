using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Rules;

public sealed class TeacherBirthDateMustBeInPastRule : IBusinessRule
{
    private readonly DateOnly _birthDate;
    private readonly DateOnly _utcNow;
    
    internal TeacherBirthDateMustBeInPastRule(DateOnly birthDate, DateOnly utcNow)
    {
        _birthDate = birthDate;
        _utcNow = utcNow;
    }
    
    public string Message =>
        "Teacher birth date must be in the past.";

    public bool IsBroken() =>
        _birthDate >= _utcNow;
}