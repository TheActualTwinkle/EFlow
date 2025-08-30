using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IBookingRepository : IRepository
{
    public Task CreateAsync(Booking booking, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Booking>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Booking>> GetBySlotIdAsync(Guid slotId, CancellationToken cancellationToken = new());

    public void Update(Booking booking);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}