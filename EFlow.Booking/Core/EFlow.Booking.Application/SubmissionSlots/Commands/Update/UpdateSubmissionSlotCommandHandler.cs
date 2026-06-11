using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public class UpdateSubmissionSlotCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock)
    : IRequestHandler<UpdateSubmissionSlotCommand, Result>
{
    public async Task<Result> Handle(UpdateSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubmissionSlotRepository>();

        var slot = await repository.GetByIdAsync(new SubmissionSlotId(request.Id), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.Id));

        var subjectId = request.Patch.SubjectId.HasValue ?
            request.Patch.SubjectId.Value! :
            slot.GetSubjectId();

        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(subjectId, cancellationToken);

        if (subject is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Subject not found")
                    .WithId(subjectId.Value));

        var admittedStudentIds = slot.GetAdmittedStudentIds().ToHashSet();
        var admittedStudents = admittedStudentIds.Count == 0
            ? []
            : (await unitOfWork
                    .GetRepository<IStudentRepository>()
                    .GetAllAsync(cancellationToken))
                .Where(student => admittedStudentIds.Contains(student.Id));

        slot.Update(
            request.Patch,
            subject.GetGroupIds(),
            subject.GetTeacherId(),
            admittedStudents,
            systemClock.UtcNow);

        repository.Update(slot);

        return Result.Ok();
    }
}
