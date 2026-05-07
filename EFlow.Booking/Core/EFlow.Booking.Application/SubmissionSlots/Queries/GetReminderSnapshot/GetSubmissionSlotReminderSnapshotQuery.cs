using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public sealed record GetSubmissionSlotReminderSnapshotQuery 
    : IRequest<Result<IEnumerable<SubmissionSlotReminderSnapshotView>>>, ICacheableRequest
