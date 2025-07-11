using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface ITeacherRepository : IRepository
{
    public Task CreateAsync(Teacher teacher, CancellationToken cancellationToken = new());

    public IEnumerable<Teacher> GetAll();

    public Task<Teacher?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Update(Teacher teacher);

    public void Delete(Teacher teacher);
}