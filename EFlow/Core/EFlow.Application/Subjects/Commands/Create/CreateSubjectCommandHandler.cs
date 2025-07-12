using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Commands;

public class CreateSubjectCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            TeacherId = request.TeacherId
        };

        await unitOfWork
            .GetRepository<ISubjectRepository>()
            .CreateAsync(subject, cancellationToken);

        return Result.Ok(subject.Id);
    }
}