using EFlow.Booking.Contracts.Students;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries.GetAllowedStudents;

public sealed record GetSubmissionSlotAllowedStudentsQuery : IRequest<Result<IEnumerable<StudentView>>>
{
    public required Guid SlotId { get; init; }
}
