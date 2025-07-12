using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Teachers.Queries;

public class GetAllTeachersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllTeachersQuery, Result<IEnumerable<TeacherDto>>>
{
    public Task<Result<IEnumerable<TeacherDto>>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var teachers = unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetAllAsync(cancellationToken)
            .Adapt<IEnumerable<TeacherDto>>();

        return Task.FromResult(Result.Ok(teachers));
    }
}