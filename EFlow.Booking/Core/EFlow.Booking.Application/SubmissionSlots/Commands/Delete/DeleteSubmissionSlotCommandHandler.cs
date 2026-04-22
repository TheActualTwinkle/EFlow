using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class DeleteSubmissionSlotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSubmissionSlotCommand, Result>
{
    public async Task<Result> Handle(DeleteSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubmissionSlotRepository>();
        
        var slot = await repository.GetByIdAsync(new SubmissionSlotId(request.Id), cancellationToken);

        if (slot is null)
            return Result.Ok();

        slot.Delete();

        await repository.DeleteAsync(slot);

        return Result.Ok();
    }
}
