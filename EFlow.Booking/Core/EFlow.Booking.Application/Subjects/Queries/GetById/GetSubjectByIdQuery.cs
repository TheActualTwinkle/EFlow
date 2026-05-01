using EFlow.Booking.Contracts.Subjects;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetSubjectByIdQuery : IRequest<Result<SubjectView>>
{
    public required Guid Id { get; init; }
}