using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Commands;

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
            TeacherId = request.TeacherId,
            GroupIds = request.GroupIds
        };

        await unitOfWork
            .GetRepository<ISubjectRepository>()
            .CreateAsync(subject, cancellationToken);

        return Result.Ok(subject.Id);
    }
}