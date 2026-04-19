using EFlow.Booking.Domain.BookingRecords.Events;
using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.BookingRecords;

public sealed class BookingRecord : Entity, IAggreagateRoot
{
    public BookingRecordId Id { get; private set; }

    internal StudentId StudentId { get; private set; }

    internal SubmissionSlotId SlotId { get; private set; }

    internal DateTime CreatedAt { get; private set; }
    
    private BookingRecord() { }
    
    private BookingRecord(
        StudentId studentId,
        SubmissionSlotId slotId,
        DateTime createdAt,
        DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(createdAt, utcNow));
        
        Id = new BookingRecordId(Guid.CreateVersion7());
        StudentId = studentId;
        SlotId = slotId;
        CreatedAt = createdAt;
    }
    public SubmissionSlotId GetSlotId() => 
        SlotId;
    
    internal static BookingRecord Create(
        StudentId studentId,
        SubmissionSlotId slotId,
        DateTime createdAt,
        DateTime utcNow)
    {
        var bookingRecord = new BookingRecord(studentId, slotId, createdAt, utcNow);
        
        bookingRecord.AddDomainEvent(new BookingRecordCreatedDomainEvent
        {
            BookingRecordId = bookingRecord.Id,
            CreatedAt = createdAt
        });
        
        return bookingRecord;
    }

    internal BookingRecordId Delete()
    {
        AddDomainEvent(new BookingRecordDeletedDomainEvent
        {
            BookingRecordId = Id,
        });

        return Id;
    }
}