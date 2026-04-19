using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule : IBusinessRule
{
    private readonly bool _allowAllGroups;
    private readonly IEnumerable<GroupId> _allowedGroupIds;
    
    internal AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule(bool allowAllGroups, IEnumerable<GroupId> allowedGroupIds)
    {
        _allowAllGroups = allowAllGroups;
        _allowedGroupIds = allowedGroupIds;
    }
    
    public string Message =>
        "Allowed group IDs must be empty when allow all groups is true.";

    public bool IsBroken() =>
        _allowAllGroups && _allowedGroupIds.Any();
}