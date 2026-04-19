using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class CreateSubmissionSlotCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock) : IRequestHandler<CreateSubmissionSlotCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var allowedGroupIds = request.AllowedGroupIds?
            .Select(id => new GroupId(id))
            .ToArray();

        var slot = SubmissionSlot.Create(
            new SubjectId(request.SubjectId),
            request.StartTime,
            request.EndTime,
            request.MaxStudents,
            request.AllowAllGroups,
            systemClock.UtcNow,
            allowedGroupIds,
            request.Location);

        await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .CreateAsync(slot, cancellationToken);

        return Result.Ok(slot.Id.Value);
    }
}