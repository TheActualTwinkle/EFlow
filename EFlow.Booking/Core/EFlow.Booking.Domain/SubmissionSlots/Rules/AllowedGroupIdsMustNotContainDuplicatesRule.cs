using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class AllowedGroupIdsMustNotContainDuplicatesRule : IBusinessRule
{
    private readonly IEnumerable<GroupId> _allowedGroupIds;

    internal AllowedGroupIdsMustNotContainDuplicatesRule(IEnumerable<GroupId> allowedGroupIds) =>
        _allowedGroupIds = allowedGroupIds;

    public string Message =>
        "Allowed group IDs must not contain duplicates.";

    public bool IsBroken() =>
        _allowedGroupIds.Count() != _allowedGroupIds.Distinct().Count();
}