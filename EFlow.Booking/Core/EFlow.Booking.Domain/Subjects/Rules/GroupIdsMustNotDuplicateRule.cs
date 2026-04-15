using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects.Rules;

/// <inheritdoc />
public sealed class GroupIdsMustNotDuplicateRule : IBusinessRule
{
    private readonly IEnumerable<GroupId> _groupIds;

    internal GroupIdsMustNotDuplicateRule(IEnumerable<GroupId> groupIds) =>
        _groupIds = groupIds;

    /// <inheritdoc />
    public string Message => 
        "Subject group IDs must not contain duplicates.";

    /// <inheritdoc />
    public bool IsBroken() =>
        _groupIds.Count() != _groupIds.Distinct().Count();
}