using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.Students;

public interface IStudentQueryService : IQueryService
{
    public Task<IEnumerable<StudentView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<StudentView?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = new());

    public Task<IEnumerable<StudentView>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = new());
}