namespace EFlow.Booking.Domain.Repositories;

public interface IBookingRecordRepository : IRepository
{
    public Task CreateAsync(Models.BookingRecord bookingRecord, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Models.BookingRecord>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Models.BookingRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Models.BookingRecord>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Models.BookingRecord>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new());

    public void Update(Models.BookingRecord bookingRecord);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}