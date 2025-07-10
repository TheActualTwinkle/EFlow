using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public class TeacherRepository(ApplicationDbContext context) :
    RepositoryBase<Teacher>(context), ITeacherRepository
{
    public async Task CreateTeacherAsync(Teacher teacher, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(teacher, cancellationToken);

    public async Task<Teacher?> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public IEnumerable<Teacher> GetAllTeachers() =>
        GetAllInternal();

    public void UpdateTeacher(Teacher teacher) =>
        UpdateInternal(teacher);

    public void DeleteTeacher(Teacher teacher) =>
        DeleteInternal(teacher);
}