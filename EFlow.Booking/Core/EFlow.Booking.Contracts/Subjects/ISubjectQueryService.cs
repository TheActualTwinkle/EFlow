using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.Subjects;

public interface ISubjectQueryService : IQueryService
{
    public Task<IEnumerable<SubjectView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<SubjectView?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<SubjectView>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new());
}