using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects;

public sealed record SubjectId(Guid Value) : TypedIdValueBase(Value);