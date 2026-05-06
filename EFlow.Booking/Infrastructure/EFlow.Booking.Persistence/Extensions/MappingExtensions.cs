using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Contracts.Teachers;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;
using EFlow.Booking.Domain.Teachers;

namespace EFlow.Booking.Persistence.Extensions;

public static class MappingExtensions
{
    extension(Student student)
    {
        public StudentView ToStudentView(Group? group = null) =>
            new()
            {
                Id = student.Id.Value,
                FirstName = student.FirstName,
                LastName = student.LastName,
                MiddleName = student.MiddleName,
                BirthDate = student.BirthDate,
                CreatedAt = student.CreatedAt,
                Group = group?.ToGroupView()
            };
    }

    extension(Teacher teacher)
    {
        public TeacherView ToTeacherView() =>
            new()
            {
                Id = teacher.Id.Value,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                MiddleName = teacher.MiddleName,
                CreatedAt = teacher.CreatedAt,
                BirthDate = teacher.BirthDate
            };
    }
    
    extension(Group group)
    {
        public GroupView ToGroupView() =>
            new()
            {
                Id = group.Id.Value,
                Name = group.Name
            };
    }

    extension(Subject subject)
    {
        public SubjectView ToSubjectView(Teacher? teacher = null, IEnumerable<Group>? groups = null) =>
            new()
            {
                Id = subject.Id.Value,
                Name = subject.Name,
                Teacher = teacher?.ToTeacherView(),
                Groups = groups?.Select(g => g.ToGroupView()).ToList()
            };
    }

    extension(SubmissionSlot submissionSlot)
    {
        public SubmissionSlotView ToSubmissionSlotView(
            Teacher? teacher = null,
            Subject? subject = null,
            IEnumerable<Group>? allowedGroups = null,
            IEnumerable<Student>? admittedStudents = null,
            int? bookingCount = null) =>
            new()
            {
                Id = submissionSlot.Id.Value,
                StartTime = submissionSlot.StartTime,
                EndTime = submissionSlot.EndTime,
                MaxStudents = submissionSlot.MaxStudents,
                BookingCount = bookingCount,
                AllowAllGroups = submissionSlot.AllowAllGroups,
                Location = submissionSlot.Location,
                Comment = submissionSlot.Comment,
                Teacher = teacher?.ToTeacherView(),
                Subject = subject?.ToSubjectView(teacher),
                AllowedGroups = allowedGroups?.Select(g => g.ToGroupView()).ToList(),
                AdmittedStudents = admittedStudents?.Select(s => s.ToStudentView()).ToList(),
                NotificationSettings = submissionSlot.NotificationSettings.Select(s => s.ToSubmissionSlotNotificationSettingsView()).ToList()
            };
    }

    extension(SubmissionSlotNotificationSettings settings)
    {
        public SubmissionSlotNotificationSettingsView ToSubmissionSlotNotificationSettingsView() =>
            new()
            {
                UserId = settings.UserId,
                SubmissionRemindTimes = settings.SubmissionRemindTimes.ToArray(),
                BookingNotificationMode = settings.BookingNotificationMode
            };
    }
    
    extension(BookingRecord bookingRecord)
    {
        public BookingRecordView ToBookingView(Student? student = null, SubmissionSlot? slot = null) =>
            new()
            {
                Id = bookingRecord.Id.Value,
                Student = student?.ToStudentView(),
                Slot = slot?.ToSubmissionSlotView(),
                CreatedAt = bookingRecord.CreatedAt
            };
    }
}
