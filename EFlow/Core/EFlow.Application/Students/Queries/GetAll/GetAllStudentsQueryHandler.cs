using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Students.Queries;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, Result<IEnumerable<StudentDto>>>
{
    public Task<Result<IEnumerable<StudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = unitOfWork
            .GetRepository<IStudentRepository>()
            .GetAllAsync(cancellationToken)
            .Adapt<IEnumerable<StudentDto>>();

        return Task.FromResult(Result.Ok(students));
    }
}