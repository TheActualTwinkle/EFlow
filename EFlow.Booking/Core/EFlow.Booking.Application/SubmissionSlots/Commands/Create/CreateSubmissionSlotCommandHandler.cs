using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class CreateSubmissionSlotCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock) : IRequestHandler<CreateSubmissionSlotCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(new SubjectId(request.SubjectId), cancellationToken);

        if (subject is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Subject not found")
                    .WithId(request.SubjectId));

        var teacher = await unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetByIdAsync(new TeacherId(request.TeacherId), cancellationToken);
        
        if (teacher is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.TeacherId));
        
        var allowedGroupIds = request.AllowedGroupIds?
            .Select(id => new GroupId(id))
            .ToList();

        var slot = SubmissionSlot.Create(
            subject.Id,
            teacher.Id,
            request.StartTime,
            request.EndTime,
            request.MaxStudents,
            request.AllowAllGroups,
            systemClock.UtcNow,
            subject.GetGroupIds(),
            subject.GetTeacherId(),
            allowedGroupIds,
            request.Location,
            request.Comment);

        await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .CreateAsync(slot, cancellationToken);

        return Result.Ok(slot.Id.Value);
    }
}
