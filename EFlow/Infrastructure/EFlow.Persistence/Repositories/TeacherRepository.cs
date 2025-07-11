using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public class TeacherRepository(ApplicationDbContext context) :
    RepositoryBase<Teacher>(context), ITeacherRepository
{
    public async Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(teacher, cancellationToken);

    public IEnumerable<Teacher> GetAll() =>
        GetAllInternal();

    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public void Update(Teacher teacher) =>
        UpdateInternal(teacher);

    public void Delete(Teacher teacher) =>
        DeleteInternal(teacher);
}