using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Teachers.Commands;

public class UpdateTeacherCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTeacherCommand, Result>
{
    public async Task<Result> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<ITeacherRepository>();

        var teacher = await repository.GetByIdAsync(request.IdentityId, cancellationToken);

        if (teacher is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Teacher not found")
                    .WithId(request.IdentityId));

        request.Adapt(teacher);

        repository.Update(teacher);

        return Result.Ok();
    }
}