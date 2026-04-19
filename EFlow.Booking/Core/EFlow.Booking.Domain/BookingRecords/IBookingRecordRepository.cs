using EFlow.Common.Domain;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;

namespace EFlow.Booking.Domain.BookingRecords;

public interface IBookingRecordRepository : IRepository
{
    public Task CreateAsync(BookingRecord bookingRecord, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<BookingRecord?> GetByIdAsync(BookingRecordId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetBySlotIdAsync(SubmissionSlotId slotId, CancellationToken cancellationToken = new());

    public void Update(BookingRecord bookingRecord);

    public Task DeleteAsync(BookingRecord bookingRecord);
}