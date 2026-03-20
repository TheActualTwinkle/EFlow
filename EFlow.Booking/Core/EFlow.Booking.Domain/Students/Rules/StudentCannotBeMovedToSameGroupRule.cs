using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Rules;

public sealed class StudentCannotBeMovedToSameGroupRule : IBusinessRule
{
    private readonly GroupId _newGroupId;
    private readonly GroupId _currentGroupId;

    public StudentCannotBeMovedToSameGroupRule(GroupId newGroupId, GroupId currentGroupId)
    {
        _newGroupId = newGroupId;
        _currentGroupId = currentGroupId;
    }

    public string Message =>
        $"Student`s new group ({_newGroupId} must no be the same as current ({_currentGroupId}).";

    public bool IsBroken() =>
        _newGroupId == _currentGroupId;
}