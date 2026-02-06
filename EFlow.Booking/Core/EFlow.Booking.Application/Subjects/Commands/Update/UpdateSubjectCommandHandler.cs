using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Commands.Update;

public class UpdateSubjectCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubjectCommand, Result>
{
    public async Task<Result> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubjectRepository>();

        var subject = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (subject is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Subject not found")
                    .WithId(request.Id));

        request.Adapt(subject);

        repository.Update(subject);

        return Result.Ok();
    }
}