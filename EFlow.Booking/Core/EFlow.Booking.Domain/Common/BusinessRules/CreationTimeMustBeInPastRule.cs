using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Common.BusinessRules;

public sealed class CreationTimeMustBeInPastRule : IBusinessRule
{
    private readonly DateTime _createdAt;
    private readonly DateTime _utcNow;

    internal CreationTimeMustBeInPastRule(DateTime createdAt, DateTime utcNow)
    {
        _createdAt = createdAt;
        _utcNow = utcNow;
    }

    public string Message =>
        "Creation time must be in the past.";

    public bool IsBroken() =>
        _createdAt > _utcNow;
}