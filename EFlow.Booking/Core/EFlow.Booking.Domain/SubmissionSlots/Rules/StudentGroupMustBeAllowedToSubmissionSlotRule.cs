using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class StudentGroupMustBeAllowedToSubmissionSlotRule : IBusinessRule
{
    private readonly bool _allowAllGroups;
    private readonly IEnumerable<GroupId> _allowedGroupIds;
    private readonly GroupId _studentGroupId;

    internal StudentGroupMustBeAllowedToSubmissionSlotRule(
        bool allowAllGroups,
        IEnumerable<GroupId> allowedGroupIds,
        GroupId studentGroupId)
    {
        _allowAllGroups = allowAllGroups;
        _allowedGroupIds = allowedGroupIds;
        _studentGroupId = studentGroupId;
    }

    public string Message => "Student group must be allowed to submission slot.";

    public bool IsBroken() =>
        !_allowAllGroups && _allowedGroupIds.All(groupId => groupId != _studentGroupId);
}
