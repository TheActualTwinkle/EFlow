using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule : IBusinessRule
{
    private readonly bool _allowAllGroups;
    private readonly IEnumerable<GroupId> _allowedGroupIds;
    
    internal AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule(bool allowAllGroups, IEnumerable<GroupId> allowedGroupIds)
    {
        _allowAllGroups = allowAllGroups;
        _allowedGroupIds = allowedGroupIds;
    }
    
    public string Message =>
        "Allowed group IDs must not be empty when allow all groups is false.";

    public bool IsBroken() =>
        !_allowAllGroups && !_allowedGroupIds.Any();
}