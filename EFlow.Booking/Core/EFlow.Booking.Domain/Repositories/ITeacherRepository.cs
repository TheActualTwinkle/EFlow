using EFlow.Booking.Domain.Models;

namespace EFlow.Booking.Domain.Repositories;

public interface ITeacherRepository : IRepository
{
    public Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Update(Teacher teacher);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}