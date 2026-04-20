using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Rules;

public sealed class GroupNameMustUniqueRule : IBusinessRule
{
    private readonly string _name;
    private readonly IEnumerable<string> _existingGroupNames;

    internal GroupNameMustUniqueRule(string name, IEnumerable<string> existingGroupNames)
    {
        _name = name;
        _existingGroupNames = existingGroupNames;
    }

    public string Message =>
        $"Group name must be unique but found another group with the '{_name}' name";

    public bool IsBroken() =>
        _existingGroupNames.Any(existingName => existingName == _name);
}
