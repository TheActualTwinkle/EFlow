using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Import;

public sealed record ImportStudentsCommand : IRequest<Result<StudentsImportResult>>
{
    public required Guid GroupId { get; init; }

    public required IReadOnlyList<ImportedStudent> Students { get; init; }
}
