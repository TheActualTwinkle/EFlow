using System.Linq.Expressions;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class BookingRecordQueryService(ApplicationDbContext context) : IBookingRecordQueryService
{
    public async Task<IEnumerable<BookingRecordView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<BookingRecordView?> GetByIdAsync(BookingRecordId id, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(r => r.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<BookingRecordView>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(r => r.StudentId == studentId)
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<BookingRecordView>> GetBySlotIdAsync(
        SubmissionSlotId slotId,
        bool fetchStudentsGroup,
        CancellationToken cancellationToken = new()) =>
        await context.BookingRecords
            .Where(r => r.SlotId == slotId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Select(MapToView(fetchStudentsGroup))
            .ToListAsync(cancellationToken);

    private Expression<Func<BookingRecord, BookingRecordView>> MapToView(bool fetchStudentsGroup = false) =>
        record => new BookingRecordView
        {
            Id = record.Id.Value,
            Student = context.Students
                .Where(s => s.Id == record.StudentId)
                .Select(s => s.ToStudentView(
                    fetchStudentsGroup ?
                        context.Groups.FirstOrDefault(g => g.Id == s.GroupId) :
                        null))
                .FirstOrDefault(),
            Slot = context.SubmissionSlots
                .Where(s => s.Id == record.SlotId)
                .Select(s => s.ToSubmissionSlotView())
                .FirstOrDefault(),
            CreatedAt = record.CreatedAt
        };
}
