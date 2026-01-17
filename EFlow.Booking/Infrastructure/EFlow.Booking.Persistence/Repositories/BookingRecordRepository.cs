using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
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
            .Where(b => b.StudentId == studentId)
            .Include(b => b.SubmissionSlot)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(b => b.SlotId == slotId)
            .Include(b => b.Student)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);

    public void Update(BookingRecord booking) =>
        UpdateInternal(booking);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}