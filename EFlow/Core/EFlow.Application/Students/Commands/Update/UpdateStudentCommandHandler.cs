using EFlow.Application.Common.Errors;
using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Students.Commands.Update;

public class UpdateStudentCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateStudentCommand, Result>
{
    public async Task<Result> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IStudentRepository>();
        
        var student = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.Id));

        request.Adapt(student);
        
        repository.Update(student);

        return Result.Ok();
    }
}