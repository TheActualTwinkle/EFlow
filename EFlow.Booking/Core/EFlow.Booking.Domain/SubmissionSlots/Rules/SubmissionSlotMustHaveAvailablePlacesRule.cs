using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class SubmissionSlotMustHaveAvailablePlacesRule : IBusinessRule
{
    private readonly int _maxStudents;
    private readonly int _bookingsCount;

    internal SubmissionSlotMustHaveAvailablePlacesRule(int maxStudents, int bookingsCount)
    {
        _maxStudents = maxStudents;
        _bookingsCount = bookingsCount;
    }

    public string Message => "Submission slot must have available places.";

    public bool IsBroken() => _bookingsCount >= _maxStudents;
}
