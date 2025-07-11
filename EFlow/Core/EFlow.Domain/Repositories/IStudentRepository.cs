using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IStudentRepository : IRepository
{
    public Task CreateAsync(Student student, CancellationToken cancellationToken = new());

    public IEnumerable<Student> GetAll();

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = new());

    public void Update(Student student);

    public void Delete(Student student);
}