using FluentValidation;

namespace EFlow.Booking.Application.Students.Commands.Import;

public sealed class ImportStudentsCommandValidator : AbstractValidator<ImportStudentsCommand>
{
    public ImportStudentsCommandValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required");

        RuleFor(x => x.Students)
            .NotEmpty().WithMessage("Students are required");
    }
}
