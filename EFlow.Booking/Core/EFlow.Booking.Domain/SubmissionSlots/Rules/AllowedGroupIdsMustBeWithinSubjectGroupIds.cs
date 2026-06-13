using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class AllowedGroupIdsMustBeWithinSubjectGroupIds : IBusinessRule
{
    private readonly IEnumerable<GroupId> _allowedGroupIds;
    private readonly IEnumerable<GroupId> _subjectGroupIds;

    internal AllowedGroupIdsMustBeWithinSubjectGroupIds(IEnumerable<GroupId> allowedGroupIds, IEnumerable<GroupId> subjectGroupIds)
    {
        _allowedGroupIds = allowedGroupIds;
        _subjectGroupIds = subjectGroupIds;
    }

    public string Message =>
        "Allowed group IDs must be within the subject's group IDs.";

    public bool IsBroken() =>
        _allowedGroupIds.Except(_subjectGroupIds).Any();
}