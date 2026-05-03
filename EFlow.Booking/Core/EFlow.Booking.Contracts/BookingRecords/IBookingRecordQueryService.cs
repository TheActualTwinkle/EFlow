using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.BookingRecords;

public interface IBookingRecordQueryService : IQueryService
{
    public Task<IEnumerable<BookingRecordView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<BookingRecordView?> GetByIdAsync(BookingRecordId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecordView>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecordView>> GetBySlotIdAsync(
        SubmissionSlotId slotId,
        bool fetchStudentsGroup,
        CancellationToken cancellationToken = new());
}