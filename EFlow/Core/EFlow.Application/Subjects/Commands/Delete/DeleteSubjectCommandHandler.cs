using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Commands;

public class DeleteSubjectCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSubjectCommand, Result>
{
    public async Task<Result> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork
            .GetRepository<ISubjectRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}