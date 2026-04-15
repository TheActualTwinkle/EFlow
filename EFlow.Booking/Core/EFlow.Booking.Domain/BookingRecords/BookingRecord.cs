using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.BookingRecords;

public sealed class BookingRecord : Entity
{
    internal BookingRecordId Id { get; private set; }

    internal StudentId StudentId { get; private set; }

    internal SubmissionSlotId SlotId { get; private set; }

    internal DateTime CreatedAt { get; private set; }
    
    
}