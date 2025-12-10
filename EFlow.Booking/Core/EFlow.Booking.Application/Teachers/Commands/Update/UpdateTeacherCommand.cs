using System.Text.Json.Serialization;
using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Commands;

public record UpdateTeacherCommand : IRequest<Result>, ITransactionalRequest
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? MiddleName { get; init; }

    public DateOnly? BirthDate { get; init; }
}