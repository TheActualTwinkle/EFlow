using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class StudentMustNotBeBookedToSubmissionSlotTwiceRule : IBusinessRule
{
    private readonly IEnumerable<BookingRecord> _bookingRecords;
    private readonly StudentId _studentId;

    internal StudentMustNotBeBookedToSubmissionSlotTwiceRule(
        IEnumerable<BookingRecord> bookingRecords,
        StudentId studentId)
    {
        _bookingRecords = bookingRecords;
        _studentId = studentId;
    }

    public string Message => "Student must not be booked to submission slot twice.";

    public bool IsBroken() => _bookingRecords.Any(bookingRecord => bookingRecord.StudentId == _studentId);
}
