using EFlow.Common.Domain.Models;

namespace EFlow.Common.Domain;

public interface IStudentRepository : IRepository
{
    public Task CreateAsync(Student student, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = new());

    public void Update(Student student);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}