using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public interface ITeacherRepository : IRepository
{
    public Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Teacher?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = new());

    public void Update(Teacher teacher);

    public Task DeleteAsync(Teacher teacher);
}