using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Application.Common.Errors.Teachers;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Teachers.Queries;

public class GetTeacherByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTeacherByIdQuery, Result<TeacherDto>>
{
    public async Task<Result<TeacherDto>> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        var teacherRepository = unitOfWork.GetRepository<ITeacherRepository>();

        var teacher = await teacherRepository.GetByIdAsync(request.Id, cancellationToken);

        if (teacher is null)
            return Result.Fail(
                new TeacherNotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.Id));

        return teacher.Adapt<TeacherDto>();
    }
}