using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Domain.Subjects;
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
        var subject = Subject.Create(
            request.Name,
            new TeacherId(request.TeacherId),
            request.GroupIds.Select(id => new GroupId(id)).ToArray());

        await unitOfWork
            .GetRepository<ISubjectRepository>()
            .CreateAsync(subject, cancellationToken);

        return Result.Ok(subject.Id.Value);
    }
}