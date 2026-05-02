using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.Teachers;

public interface ITeacherQueryService : IQueryService
{
    public Task<IEnumerable<TeacherView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<TeacherView?> GetByIdAsync(TeacherId id, CancellationToken cancellationToken = new());
}