using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Students.Queries;

public class GetAllStudentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllStudentsQuery, Result<IEnumerable<StudentDto>>>
{
    public async Task<Result<IEnumerable<StudentDto>>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = (await unitOfWork
                .GetRepository<IStudentRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<StudentDto>>();

        return Result.Ok(students);
    }
}