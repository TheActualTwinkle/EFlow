using FluentPatcher;

namespace EFlow.Booking.WebApi.Contracts.Subjects;

public record UpdateSubjectRequest
{
    public Patchable<string> Name { get; init; }

    public Patchable<Guid> TeacherId { get; init; }
    
    public Patchable<ICollection<Guid>> GroupIds { get; init; }
}
