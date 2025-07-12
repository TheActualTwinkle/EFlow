using System.Text.Json.Serialization;
using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Commands.Update;

public record UpdateBookingCommand : IRequest<Result>, ITransactionalRequest
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public Guid? StudentId { get; init; }

    public Guid? SlotId { get; init; }
}