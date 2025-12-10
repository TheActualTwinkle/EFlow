using EFlow.Booking.Domain.Models;
using EFlow.Booking.Domain.Repositories;
using EFlow.Booking.Persistence.DatabaseContext;

namespace EFlow.Booking.Persistence.Repositories;

public class TeacherRepository(ApplicationDbContext context) :
    RepositoryBase<Teacher>(context), ITeacherRepository
{
    public async Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(teacher, cancellationToken);

    public async Task<IEnumerable<Teacher>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public void Update(Teacher teacher) =>
        UpdateInternal(teacher);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await DeleteInternalAsync(id, cancellationToken);
}