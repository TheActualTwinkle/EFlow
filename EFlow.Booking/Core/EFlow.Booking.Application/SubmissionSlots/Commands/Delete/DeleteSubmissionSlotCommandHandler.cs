using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class DeleteSubmissionSlotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSubmissionSlotCommand, Result>
{
    public async Task<Result> Handle(DeleteSubmissionSlotCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}