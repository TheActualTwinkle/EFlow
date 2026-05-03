using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects.Rules;

public sealed class GroupIdsMustNotBeEmpty : IBusinessRule
{
    private readonly IEnumerable<GroupId> _groupIds;

    internal GroupIdsMustNotBeEmpty(IEnumerable<GroupId> groupIds) =>
        _groupIds = groupIds;

    public string Message =>
        "Subject must be associated with at least one group.";

    public bool IsBroken() =>
        !_groupIds.Any();
}