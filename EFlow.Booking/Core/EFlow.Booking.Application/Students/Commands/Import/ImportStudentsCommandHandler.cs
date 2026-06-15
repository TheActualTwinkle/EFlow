using EFlow.Booking.Application.Common.Errors;
using EFlow.Common.Domain.Exceptions;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Import;

public sealed class ImportStudentsCommandHandler(ISender sender)
    : IRequestHandler<ImportStudentsCommand, Result<StudentsImportResult>>
{
    // TODO: Optimize this by somehow using the single transaction for the whole import process instead of creating a new transaction for each student creation.
    // For now we are using the existing CreateStudentCommandHandler which creates a new transaction for each student creation.
    // This is because student creation involves creating a new user in the identity system and adding them to a role.
    // Also a new students batch may also have duplicate emails or usernames.
    public async Task<Result<StudentsImportResult>> Handle(
        ImportStudentsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<StudentImportRowError>();
        var importedCount = 0;

        foreach (var student in request.Students)
        {
            var createResult = await TryCreateStudentAsync(
                new CreateStudentCommand
                {
                    GroupId = request.GroupId,
                    LastName = student.LastName,
                    FirstName = student.FirstName,
                    MiddleName = student.MiddleName,
                    Email = student.Email,
                    UserName = student.UserName,
                    Password = student.Password,
                    BirthDate = student.BirthDate
                },
                cancellationToken);

            if (createResult.IsFailed)
            {
                errors.Add(
                    new StudentImportRowError
                    {
                        RowNumber = student.RowNumber,
                        Message =
                            $"{createResult.Errors[0].Message}: {createResult.Errors[0].Reasons?.FirstOrDefault()?.Message ?? "No additional details"}"
                    });

                continue;
            }

            importedCount++;
        }

        return Result.Ok(new StudentsImportResult
        {
            TotalCount = request.Students.Count,
            ImportedCount = importedCount,
            FailedCount = errors.Count,
            Errors = errors
        });
    }
    
    private async Task<Result<Guid>> TryCreateStudentAsync(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            return await sender.Send(command, cancellationToken);
        }
        catch (BusinessRuleValidationException exception)
        {
            return Result.Fail(new BadRequestError
            {
                Message = exception.BrokenRule.Message
            });
        }
    }
}
