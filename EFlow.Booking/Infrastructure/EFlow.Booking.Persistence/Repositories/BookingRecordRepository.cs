using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class BookingRecordRepository(ApplicationDbContext context) :
    IBookingRecordRepository
{
    public async Task CreateAsync(BookingRecord booking, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords.AddAsync(booking, cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.BookingRecords.ToListAsync(cancellationToken);

    public async Task<BookingRecord?> GetByIdAsync(BookingRecordId id, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(b => b.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<BookingRecord>> GetBySlotIdAsync(SubmissionSlotId slotId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(b => b.SlotId == slotId)
            .OrderBy(b => b.CreatedAt)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);

    public void Update(BookingRecord booking) =>
        context.BookingRecords.Update(booking);

    public Task DeleteAsync(BookingRecord bookingRecord)
    {
        context.BookingRecords.Remove(bookingRecord);
        return Task.CompletedTask;
    }
}