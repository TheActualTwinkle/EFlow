using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain.Exceptions;
using FluentAssertions;
using FluentPatcher;

namespace EFlow.Booking.UnitTests.Domain.SubmissionSlots;

public class SubmissionSlotTests
{
    [Fact]
    public void Create_WhenTeacherDoesNotOwnSubject_ShouldThrowBusinessRuleValidationException()
    {
        // Arrange
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var groupId = new GroupId(Guid.CreateVersion7());
        var slotTeacherId = new TeacherId(Guid.CreateVersion7());
        var subjectTeacherId = new TeacherId(Guid.CreateVersion7());

        // Act
        var act = () => SubmissionSlot.Create(
            new SubjectId(Guid.CreateVersion7()),
            slotTeacherId,
            new DateTime(2026, 05, 12, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 05, 12, 12, 0, 0, DateTimeKind.Utc),
            5,
            true,
            now,
            [groupId],
            subjectTeacherId);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .Which.BrokenRule.GetType().Name.Should().Be("TeacherMustOwnSubjectRule");
    }

    [Fact]
    public void CancelBooking_WhenStudentHasNotificationSettings_ShouldRemoveOnlyThatStudentsSettings()
    {
        // Arrange
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var groupId = new GroupId(Guid.CreateVersion7());
        var studentId = new StudentId(Guid.CreateVersion7());
        var teacherId = new TeacherId(Guid.CreateVersion7());
        var otherUserId = Guid.CreateVersion7();
        var slot = SubmissionSlot.Create(
            new SubjectId(Guid.CreateVersion7()),
            teacherId,
            new DateTime(2026, 05, 12, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 05, 12, 12, 0, 0, DateTimeKind.Utc),
            5,
            true,
            now,
            [groupId],
            teacherId);
        
        var student = Student.Create(
            studentId,
            groupId,
            "Ivan",
            "Petrov",
            null,
            new DateOnly(2005, 05, 01),
            now,
            now);

        slot.AddAdmission(studentId, student.GetGroupId());
        slot.UpdateNotificationSettings(studentId.Value, [], [SubmissionRemindTime.OneWeek], BookingNotificationMode.All);
        slot.UpdateNotificationSettings(otherUserId, [], [SubmissionRemindTime.TwoDays], BookingNotificationMode.OnlyCancellation);
        var booking = slot.BookToSlot(student, Array.Empty<BookingRecord>(), now);

        // Act
        slot.CancelBooking(booking, now.AddMinutes(5));

        // Assert
        var recipients = slot.GetNotificationRecipients();
        recipients.Should().ContainSingle();
        recipients.Should().ContainSingle(recipient =>
            recipient.UserId == otherUserId &&
            recipient.BookingNotificationMode == BookingNotificationMode.OnlyCancellation);
        recipients.Should().NotContain(recipient => recipient.UserId == studentId.Value);
    }

    [Fact]
    public void Update_WhenAllowedGroupsExcludeAdmittedStudentGroup_ShouldRemoveAdmission()
    {
        // Arrange
        var now = new DateTime(2026, 04, 28, 12, 0, 0, DateTimeKind.Utc);
        var allowedGroupId = new GroupId(Guid.CreateVersion7());
        var excludedGroupId = new GroupId(Guid.CreateVersion7());
        var teacherId = new TeacherId(Guid.CreateVersion7());
        var allowedStudent = Student.Create(
            new StudentId(Guid.CreateVersion7()),
            allowedGroupId,
            "Ivan",
            "Petrov",
            null,
            new DateOnly(2005, 05, 01),
            now,
            now);
        var excludedStudent = Student.Create(
            new StudentId(Guid.CreateVersion7()),
            excludedGroupId,
            "Petr",
            "Ivanov",
            null,
            new DateOnly(2005, 05, 01),
            now,
            now);
        var slot = SubmissionSlot.Create(
            new SubjectId(Guid.CreateVersion7()),
            teacherId,
            new DateTime(2026, 05, 12, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 05, 12, 12, 0, 0, DateTimeKind.Utc),
            5,
            true,
            now,
            [allowedGroupId, excludedGroupId],
            teacherId);

        slot.AddAdmission(allowedStudent.Id, allowedStudent.GetGroupId());
        slot.AddAdmission(excludedStudent.Id, excludedStudent.GetGroupId());

        // Act
        slot.Update(
            new SubmissionSlotPatch
            {
                AllowAllGroups = Patchable.Set(false),
                AllowedGroupIds = Patchable.Set<List<GroupId>>([allowedGroupId])
            },
            [allowedGroupId, excludedGroupId],
            teacherId,
            [allowedStudent, excludedStudent],
            new Dictionary<Student, BookingRecord>(),
            now);

        // Assert
        slot.GetAdmittedStudentIds().Should().ContainSingle().Which.Should().Be(allowedStudent.Id);
    }
}
