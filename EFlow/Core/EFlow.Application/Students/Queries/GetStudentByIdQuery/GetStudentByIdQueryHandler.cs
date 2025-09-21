using EFlow.Application.Common.Errors;
using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Students.Queries;

public class GetStudentByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.Id));

        return student.Adapt<StudentDto>();
    }
}