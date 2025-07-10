using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface ITeacherRepository : IRepository
{
    public Task CreateTeacherAsync(Teacher teacher, CancellationToken cancellationToken = new());
    
    public Task<Teacher?> GetTeacherByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public IEnumerable<Teacher> GetAllTeachers();
    
    public void UpdateTeacher(Teacher teacher);
    
    public void DeleteTeacher(Teacher teacher);
}