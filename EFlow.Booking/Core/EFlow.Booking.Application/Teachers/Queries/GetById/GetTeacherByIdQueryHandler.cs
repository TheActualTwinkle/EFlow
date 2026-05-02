using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Teachers;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public class GetTeacherByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTeacherByIdQuery, Result<TeacherView>>
{
    public async Task<Result<TeacherView>> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        var teacher = await unitOfWork
            .GetQueryService<ITeacherQueryService>()
            .GetByIdAsync(new TeacherId(request.Id), cancellationToken);

        if (teacher is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.Id));

        return Result.Ok(teacher);
    }
}