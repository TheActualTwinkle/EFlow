using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class BookingRecordRepository(ApplicationDbContext context) :
    RepositoryBase<BookingRecord>(context), IBookingRecordRepository
{
    public async Task CreateAsync(BookingRecord booking, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(booking, cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<BookingRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(b => b.StudentId.Value == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(b => b.SlotId.Value == slotId)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);

    public void Update(BookingRecord booking) =>
        UpdateInternal(booking);

    public Task DeleteAsync(BookingRecord bookingRecord) =>
        DeleteInternalAsync(bookingRecord);
}