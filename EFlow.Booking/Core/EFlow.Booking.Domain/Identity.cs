using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Domain;

public sealed class Identity : IdentityUser<Guid>
{
    public enum Role
    {
        Admin,
        Teacher,
        Student
    }

    public static string GetRoleName(Role role) =>
        role switch
        {
            Role.Admin => Roles.Admin,
            Role.Teacher => Roles.Teacher,
            Role.Student => Roles.Student,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        public static IEnumerable<string> GetAll() =>
            [Admin, Teacher, Student];
    }
}