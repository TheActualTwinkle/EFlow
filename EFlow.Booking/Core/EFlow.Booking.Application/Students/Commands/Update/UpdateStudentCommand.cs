using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Update;

public record UpdateStudentCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public Guid? GroupId { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? MiddleName { get; init; }

    public DateOnly? BirthDate { get; init; }
}
