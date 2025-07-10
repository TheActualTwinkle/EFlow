using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Application.Common.Errors.Identity;
using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Application.Teachers.Commands;

public class DeleteTeacherCommandHandler(IUnitOfWork unitOfWork, UserManager<Identity> userManager) : IRequestHandler<DeleteTeacherCommand, Result>
{
    public async Task<Result> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var identity = await userManager.FindByIdAsync(request.Id.ToString());

        if (identity is null)
            return Result.Ok();

        var result = await userManager.DeleteAsync(identity);

        if (!result.Succeeded)
            return Result.Fail(
                new IdentityInternalError()
                    .WithMessage("Failed to delete user")
                    .WithIdentityErrors(result.Errors));

        var teacherRepository = unitOfWork.GetRepository<ITeacherRepository>();

        var teacher = await teacherRepository.GetTeacherByIdAsync(request.Id, cancellationToken);

        if (teacher is null)
            return Result.Ok();

        teacherRepository.DeleteTeacher(teacher);

        return Result.Ok();
    }
}