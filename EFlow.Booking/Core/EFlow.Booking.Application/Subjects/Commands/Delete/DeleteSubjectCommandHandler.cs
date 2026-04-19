using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Commands;

public class DeleteSubjectCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteSubjectCommand, Result>
{
    public async Task<Result> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ISubjectRepository>();
        
        var subject = await repository.GetByIdAsync(new SubjectId(request.Id), cancellationToken);

        if (subject is null)
            return Result.Ok();

        subject.Delete();

        await repository.DeleteAsync(subject);

        return Result.Ok();
    }
}