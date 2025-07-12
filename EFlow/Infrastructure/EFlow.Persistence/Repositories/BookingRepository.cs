using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class BookingRepository(ApplicationDbContext context) :
    RepositoryBase<Booking>(context), IBookingRepository
{
    public async Task CreateAsync(Booking booking, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(booking, cancellationToken);

    public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public async Task<IEnumerable<Booking>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = new()) =>
        await Context.Bookings
            .Where(b => b.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Booking>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new()) =>
        await Context.Bookings
            .Where(b => b.SlotId == slotId)
            .ToListAsync(cancellationToken);

    public void Update(Booking booking) =>
        UpdateInternal(booking);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}