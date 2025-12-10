using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public class UpdateSubmissionSlotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubmissionSlotCommand, Result>
{
    public async Task<Result> Handle(UpdateSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubmissionSlotRepository>();

        var slot = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.Id));

        request.Adapt(slot);

        repository.Update(slot);

        return Result.Ok();
    }
}