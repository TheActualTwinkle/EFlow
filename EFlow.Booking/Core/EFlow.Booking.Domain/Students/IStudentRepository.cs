using EFlow.Common.Domain;
using EFlow.Booking.Domain.Groups;

namespace EFlow.Booking.Domain.Students;

public interface IStudentRepository : IRepository
{
    public Task CreateAsync(Student student, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Student>> GetByGroupIdsAsync(IEnumerable<GroupId> groupIds, CancellationToken cancellationToken = new());

    public void Update(Student student);

    public Task DeleteAsync(Student student);
}
