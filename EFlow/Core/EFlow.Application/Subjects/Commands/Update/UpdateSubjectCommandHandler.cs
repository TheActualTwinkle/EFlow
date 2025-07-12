using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Subjects.Commands.Update;

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