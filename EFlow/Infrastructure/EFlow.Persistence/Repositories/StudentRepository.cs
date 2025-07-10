using EFlow.Domain.Models;
using EFlow.Persistence.DatabaseContext;
using EFlow.Domain.Repositories;

namespace EFlow.Persistence.Repositories;

public class StudentRepository(ApplicationDbContext context) :
    RepositoryBase<Student>(context), IStudentRepository;