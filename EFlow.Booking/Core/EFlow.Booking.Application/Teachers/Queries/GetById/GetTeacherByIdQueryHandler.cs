using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public class GetTeacherByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTeacherByIdQuery, Result<TeacherDto>>
{
    public async Task<Result<TeacherDto>> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        var teacher = await unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetByIdAsync(new TeacherId(request.Id), cancellationToken);

        if (teacher is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.Id));

        return teacher.Adapt<TeacherDto>();
    }
}