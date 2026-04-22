using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class AddAdmissionCommandHandler(
    IUnitOfWork unitOfWork,
    ISystemClock systemClock)
    : IRequestHandler<AddAdmissionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddAdmissionCommand request, CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken);

        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission slot not found")
                    .WithId(request.SlotId));

        var student = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByIdAsync(new StudentId(request.StudentId), cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.StudentId));

        var admission = slot.AddAdmission(student.Id, systemClock.UtcNow);
        
        return Result.Ok(admission.Id.Value);
    }
}
