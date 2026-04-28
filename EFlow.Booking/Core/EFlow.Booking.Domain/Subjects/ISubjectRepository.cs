using EFlow.Common.Domain;
using EFlow.Booking.Domain.Teachers;

namespace EFlow.Booking.Domain.Subjects;

public interface ISubjectRepository : IRepository
{
    public Task CreateAsync(Subject subject, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Subject?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Subject>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new());

    public void Update(Subject subject);

    public Task DeleteAsync(Subject subject);
}