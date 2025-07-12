using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface ISubjectRepository : IRepository
{
    public Task CreateAsync(Subject subject, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Subject>> GetByTeacherIdAsync(Guid teacherId, CancellationToken cancellationToken = new());

    public void Update(Subject subject);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}