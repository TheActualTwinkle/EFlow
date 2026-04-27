using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class UpdateNotificationSettingsCommandHandler(
    IUnitOfWork unitOfWork,
    ISystemClock systemClock,
    UserManager<Identity> userManager)
    : IRequestHandler<UpdateNotificationSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubmissionSlotRepository>();
        var slot = await repository.GetByIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.SlotId));

        var user = await userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("User not found")
                    .WithId(request.UserId));

        slot.UpdateNotificationSettings(
            request.UserId,
            request.SubmissionRemindTimes,
            request.BookingNotificationMode,
            systemClock.UtcNow);

        repository.Update(slot);

        return Result.Ok();
    }
}
