using EFlow.Booking.Domain.Models;

namespace EFlow.Booking.Domain.Repositories;

public interface IBookingRecordRepository : IRepository
{
    public Task CreateAsync(BookingRecord bookingRecord, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<BookingRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<BookingRecord>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new());

    public void Update(BookingRecord bookingRecord);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}