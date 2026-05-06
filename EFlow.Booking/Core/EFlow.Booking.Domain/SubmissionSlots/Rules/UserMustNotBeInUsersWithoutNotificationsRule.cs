using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Rules;

public sealed class UserMustNotBeInUsersWithoutNotificationsRule : IBusinessRule
{
    private readonly Guid _userId;
    private readonly ICollection<Guid> _usersWithoutNotifications;

    internal UserMustNotBeInUsersWithoutNotificationsRule(Guid userId, ICollection<Guid> usersWithoutNotifications)
    {
        _userId = userId;
        _usersWithoutNotifications = usersWithoutNotifications;
    }

    public string Message => "User must not be in users without notifications.";

    public bool IsBroken() =>
        _usersWithoutNotifications.Contains(_userId);
}