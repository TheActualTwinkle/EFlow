using System.Data;
using EFlow.Booking.Application.BookingRecords.Notifications;
using EFlow.Booking.Application.Common.Outbox;
using EFlow.Booking.Application.SubmissionSlots.Notifications;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.BookingRecords.Events;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Entities;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using FluentAssertions;
using MemoryPack;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EFlow.Booking.IntegrationTests.Application.Notifications;

public class BookingNotificationHandlersIntegrationTests
{
    private const string DefaultGroupName = "M8O-411B-22";
    private const string DefaultLocation = "A-101";
    private const string DefaultComment = "Bring slides";
    private const string TeacherFirstName = "Ivan";
    private const string TeacherLastName = "Petrov";
    private const string AllNotificationsEmail = "all@example.com";
    private const string CreatedNotificationsEmail = "created@example.com";
    private const string CancelledNotificationsEmail = "cancelled@example.com";
    private const string SkippedNotificationsEmail = "skipped@example.com";
    private const string FirstStudentFirstName = "Petr";
    private const string FirstStudentLastName = "Ivanov";
    private const string SecondStudentFirstName = "Sergey";
    private const string SecondStudentLastName = "Petrov";
    private const string NoEmailStudentFirstName = "No";
    private const string NoEmailStudentLastName = "Email";
    private const string CancelledBookingStudentFirstName = "Nikita";
    private const string CancelledBookingStudentLastName = "Volkov";
    private const int DefaultMaxStudents = 5;
    private static readonly DateTime CreatedAt = Utc(2026, 04, 28, 11);
    private static readonly DateTime Now = Utc(2026, 04, 28, 12);
    private static readonly DateTime DefaultSlotStart = Utc(2026, 05, 12, 10);
    private static readonly DateTime DefaultSlotEnd = Utc(2026, 05, 12, 12);

    [Fact]
    public async Task Publish_WhenSubmissionSlotCreated_ShouldCreateOutboxMessageWithRecipientsFromAllowedGroups()
    {
        // Arrange
        const string subjectName = "Distributed Systems";
        const string recipientEmail = "recipient@example.com";
        var allowedGroup = Group.Create(DefaultGroupName, []);
        var skippedGroup = Group.Create("M8O-412B-22", []);
        var teacher = CreateTeacher(Guid.NewGuid(), TeacherFirstName, TeacherLastName);
        var subject = Subject.Create(subjectName, teacher.Id, [allowedGroup.Id, skippedGroup.Id]);

        var recipientStudent = CreateStudent(Guid.NewGuid(), allowedGroup.Id, FirstStudentFirstName, FirstStudentLastName);
        var skippedStudent = CreateStudent(Guid.NewGuid(), skippedGroup.Id, SecondStudentFirstName, SecondStudentLastName);
        var noEmailStudent = CreateStudent(Guid.NewGuid(), allowedGroup.Id, NoEmailStudentFirstName, NoEmailStudentLastName);

        var slot = SubmissionSlot.Create(
            subject.Id, teacher.Id, DefaultSlotStart, DefaultSlotEnd, DefaultMaxStudents, false, Now, [allowedGroup.Id], DefaultLocation,
            DefaultComment);

        var domainEvent = slot.DequeueDomainEvents().OfType<SubmissionSlotCreatedDomainEvent>().Single();
        var outboxRepository = new FakeOutboxMessageRepository();

        var handler = new SubmissionSlotCreatedDomainEventNotificationHandler(
            CreateUnitOfWork(
                CreateTeacherRepository(teacher),
                CreateSubjectRepository(subject),
                CreateGroupRepository(allowedGroup, skippedGroup),
                CreateStudentRepository(
                    (recipientStudent, allowedGroup.Id),
                    (skippedStudent, skippedGroup.Id),
                    (noEmailStudent, allowedGroup.Id)),
                outboxRepository: outboxRepository),
            new OutboxMessageFactory(),
            CreateUserManager(
                CreateIdentity(recipientStudent.Id.Value, recipientEmail),
                CreateIdentity(skippedStudent.Id.Value, SkippedNotificationsEmail),
                CreateIdentity(noEmailStudent.Id.Value, null)),
            NullLogger<SubmissionSlotCreatedDomainEventNotificationHandler>.Instance);

        // Act
        await handler.Handle(
            new SubmissionSlotCreatedDomainEventNotification
            {
                DomainEvent = domainEvent
            },
            CancellationToken.None);

        var outboxMessage = outboxRepository.Messages.Should().ContainSingle().Subject;
        var integrationEvent = MemoryPackSerializer.Deserialize<SubmissionSlotCreatedIntegrationEvent>(outboxMessage.Payload);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.SubmissionSlot.SubjectName.Should().Be(subjectName);
        integrationEvent.SubmissionSlot.AllowedGroups.Select(group => group.Name).Should().BeEquivalentTo(DefaultGroupName);

        integrationEvent.NotificationRecipients.Should().ContainSingle(x =>
            x.UserId == recipientStudent.Id.Value &&
            x.Email == recipientEmail);
    }

    [Fact]
    public async Task Publish_WhenSubmissionSlotUpdated_ShouldCreateOutboxMessageWithRecipientsFromOldAndNewAllowedGroups()
    {
        // Arrange
        const string newGroupName = "M8O-413B-22";
        const string oldGroupEmail = "old-group@example.com";
        const string newGroupEmail = "new-group@example.com";
        var oldSlotStart = Utc(2026, 05, 10, 10);
        var oldSlotEnd = Utc(2026, 05, 10, 12);
        var oldGroup = Group.Create(DefaultGroupName, []);
        var newGroup = Group.Create(newGroupName, []);
        var skippedGroup = Group.Create("M8O-414B-22", []);
        var teacher = CreateTeacher(Guid.NewGuid(), TeacherFirstName, TeacherLastName);
        var subject = Subject.Create("Algorithms", teacher.Id, [oldGroup.Id, newGroup.Id, skippedGroup.Id]);

        var oldGroupStudent = CreateStudent(Guid.NewGuid(), oldGroup.Id, FirstStudentFirstName, FirstStudentLastName);
        var newGroupStudent = CreateStudent(Guid.NewGuid(), newGroup.Id, SecondStudentFirstName, SecondStudentLastName);
        var skippedStudent = CreateStudent(Guid.NewGuid(), skippedGroup.Id, CancelledBookingStudentFirstName, CancelledBookingStudentLastName);
        var noEmailStudent = CreateStudent(Guid.NewGuid(), oldGroup.Id, NoEmailStudentFirstName, NoEmailStudentLastName);

        var slot = SubmissionSlot.Create(
            subject.Id, teacher.Id, oldSlotStart, oldSlotEnd, DefaultMaxStudents, false, Now, [oldGroup.Id], DefaultLocation,
            DefaultComment);

        slot.DequeueDomainEvents();

        var domainEvent = new SubmissionSlotUpdatedDomainEvent
        {
            SlotId = slot.Id,
            UpdatedAt = Utc(2026, 04, 28, 12, 30),
            OldSlot = new SubmissionSlotSnapshot
            {
                SlotId = slot.Id,
                SubjectId = subject.Id,
                TeacherId = teacher.Id,
                StartTime = oldSlotStart,
                EndTime = oldSlotEnd,
                MaxStudents = DefaultMaxStudents,
                Location = DefaultLocation,
                Comment = DefaultComment,
                AllowAllGroups = false,
                AllowedGroupIds = [oldGroup.Id]
            },
            NewSlot = new SubmissionSlotSnapshot
            {
                SlotId = slot.Id,
                SubjectId = subject.Id,
                TeacherId = teacher.Id,
                StartTime = Utc(2026, 05, 12, 14),
                EndTime = Utc(2026, 05, 12, 16),
                MaxStudents = 6,
                Location = "B-204",
                Comment = "Updated comment",
                AllowAllGroups = false,
                AllowedGroupIds = [newGroup.Id]
            }
        };

        var outboxRepository = new FakeOutboxMessageRepository();

        var handler = new SubmissionSlotUpdatedDomainEventNotificationHandler(
            CreateUnitOfWork(
                submissionSlotRepository: CreateSubmissionSlotRepository(slot),
                teacherRepository: CreateTeacherRepository(teacher),
                subjectRepository: CreateSubjectRepository(subject),
                groupRepository: CreateGroupRepository(oldGroup, newGroup, skippedGroup),
                studentRepository: CreateStudentRepository(
                    (oldGroupStudent, oldGroup.Id),
                    (newGroupStudent, newGroup.Id),
                    (skippedStudent, skippedGroup.Id),
                    (noEmailStudent, oldGroup.Id)),
                outboxRepository: outboxRepository),
            new OutboxMessageFactory(),
            CreateUserManager(
                CreateIdentity(oldGroupStudent.Id.Value, oldGroupEmail),
                CreateIdentity(newGroupStudent.Id.Value, newGroupEmail),
                CreateIdentity(skippedStudent.Id.Value, SkippedNotificationsEmail),
                CreateIdentity(noEmailStudent.Id.Value, null)),
            NullLogger<SubmissionSlotUpdatedDomainEventNotificationHandler>.Instance);

        // Act
        await handler.Handle(
            new SubmissionSlotUpdatedDomainEventNotification
            {
                DomainEvent = domainEvent
            },
            CancellationToken.None);

        var outboxMessage = outboxRepository.Messages.Should().ContainSingle().Subject;
        var integrationEvent = MemoryPackSerializer.Deserialize<SubmissionSlotUpdatedIntegrationEvent>(outboxMessage.Payload);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.OldSubmissionSlot.AllowedGroups.Select(group => group.Name).Should().BeEquivalentTo(DefaultGroupName);
        integrationEvent.NewSubmissionSlot.AllowedGroups.Select(group => group.Name).Should().BeEquivalentTo(newGroupName);

        integrationEvent.NotificationRecipients
            .Select(x => new { x.Email, x.UserId })
            .Should()
            .BeEquivalentTo(
            [
                new { Email = oldGroupEmail, UserId = oldGroupStudent.Id.Value },
                new { Email = newGroupEmail, UserId = newGroupStudent.Id.Value }
            ]);
    }

    [Fact]
    public async Task Publish_WhenBookingCreated_ShouldUseOnlyAllAndOnlyNewBookingModes()
    {
        // Arrange
        var group = Group.Create(DefaultGroupName, []);
        var teacher = CreateTeacher(Guid.NewGuid(), TeacherFirstName, TeacherLastName);
        var subject = Subject.Create("Databases", teacher.Id, [group.Id]);
        var bookedStudent = CreateStudent(Guid.NewGuid(), group.Id, "Artem", "Fedorov");

        var slot = SubmissionSlot.Create(subject.Id, teacher.Id, DefaultSlotStart, DefaultSlotEnd, DefaultMaxStudents, true, Now);
        slot.DequeueDomainEvents();

        var notifyAllId = Guid.NewGuid();
        var notifyCreatedOnlyId = Guid.NewGuid();
        var notifyCancelledOnlyId = Guid.NewGuid();
        slot.UpdateNotificationSettings(notifyAllId, [], [SubmissionRemindTime.OneWeek], BookingNotificationMode.All);

        slot.UpdateNotificationSettings(
            notifyCreatedOnlyId, [], [SubmissionRemindTime.TwoDays], BookingNotificationMode.OnlyNewBooking);

        slot.UpdateNotificationSettings(
            notifyCancelledOnlyId, [], [SubmissionRemindTime.FourHours], BookingNotificationMode.OnlyCancellation);

        slot.AddAdmission(bookedStudent.Id);
        var bookingRecord = slot.BookToSlot(bookedStudent, [], Now);
        var domainEvent = bookingRecord.DequeueDomainEvents().OfType<BookingRecordCreatedDomainEvent>().Single();
        var outboxRepository = new FakeOutboxMessageRepository();

        var handler = new BookingRecordCreatedDomainEventNotificationHandler(
            CreateUnitOfWork(
                submissionSlotRepository: CreateSubmissionSlotRepository(slot),
                studentRepository: CreateStudentRepository((bookedStudent, group.Id)),
                teacherRepository: CreateTeacherRepository(teacher),
                subjectRepository: CreateSubjectRepository(subject),
                groupRepository: CreateGroupRepository(group),
                outboxRepository: outboxRepository),
            new OutboxMessageFactory(),
            CreateUserManager(
                CreateIdentity(notifyAllId, AllNotificationsEmail),
                CreateIdentity(notifyCreatedOnlyId, CreatedNotificationsEmail),
                CreateIdentity(notifyCancelledOnlyId, CancelledNotificationsEmail)),
            NullLogger<BookingRecordCreatedDomainEventNotificationHandler>.Instance);

        // Act
        await handler.Handle(
            new BookingRecordCreatedDomainEventNotification
            {
                DomainEvent = domainEvent
            },
            CancellationToken.None);

        var outboxMessage = outboxRepository.Messages.Should().ContainSingle().Subject;
        var integrationEvent = MemoryPackSerializer.Deserialize<BookingCreatedIntegrationEvent>(outboxMessage.Payload);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.BookingRecord.StudentFullName.Should().Be("Fedorov Artem");

        integrationEvent.NotificationRecipients
            .Select(x => new { x.Email, x.UserId })
            .Should()
            .BeEquivalentTo(
            [
                new { Email = AllNotificationsEmail, UserId = notifyAllId },
                    new { Email = CreatedNotificationsEmail, UserId = notifyCreatedOnlyId }
            ]);
    }

    [Fact]
    public async Task Publish_WhenBookingCancelled_ShouldUseOnlyAllAndOnlyCancellationModes()
    {
        // Arrange
        var group = Group.Create(DefaultGroupName, []);
        var teacher = CreateTeacher(Guid.NewGuid(), TeacherFirstName, TeacherLastName);
        var subject = Subject.Create("Operating Systems", teacher.Id, [group.Id]);
        var bookedStudent = CreateStudent(Guid.NewGuid(), group.Id, CancelledBookingStudentFirstName, CancelledBookingStudentLastName);

        var slot = SubmissionSlot.Create(subject.Id, teacher.Id, DefaultSlotStart, DefaultSlotEnd, DefaultMaxStudents, true, Now);
        slot.DequeueDomainEvents();

        var notifyAllId = Guid.NewGuid();
        var notifyCreatedOnlyId = Guid.NewGuid();
        var notifyCancelledOnlyId = Guid.NewGuid();
        slot.UpdateNotificationSettings(notifyAllId, [], [SubmissionRemindTime.OneWeek], BookingNotificationMode.All);

        slot.UpdateNotificationSettings(
            notifyCreatedOnlyId, [], [SubmissionRemindTime.TwoDays], BookingNotificationMode.OnlyNewBooking);

        slot.UpdateNotificationSettings(
            notifyCancelledOnlyId, [], [SubmissionRemindTime.FourHours], BookingNotificationMode.OnlyCancellation);

        slot.AddAdmission(bookedStudent.Id);
        var bookingRecord = slot.BookToSlot(bookedStudent, [], Now);
        bookingRecord.DequeueDomainEvents();
        slot.CancelBooking(bookingRecord, Utc(2026, 04, 28, 13));
        var domainEvent = bookingRecord.DequeueDomainEvents().OfType<BookingRecordDeletedDomainEvent>().Single();
        var outboxRepository = new FakeOutboxMessageRepository();

        var handler = new BookingRecordDeletedDomainEventNotificationHandler(
            CreateUnitOfWork(
                submissionSlotRepository: CreateSubmissionSlotRepository(slot),
                studentRepository: CreateStudentRepository((bookedStudent, group.Id)),
                teacherRepository: CreateTeacherRepository(teacher),
                subjectRepository: CreateSubjectRepository(subject),
                groupRepository: CreateGroupRepository(group),
                outboxRepository: outboxRepository),
            new OutboxMessageFactory(),
            CreateUserManager(
                CreateIdentity(notifyAllId, AllNotificationsEmail),
                CreateIdentity(notifyCreatedOnlyId, CreatedNotificationsEmail),
                CreateIdentity(notifyCancelledOnlyId, CancelledNotificationsEmail)),
            NullLogger<BookingRecordDeletedDomainEventNotificationHandler>.Instance);

        // Act
        await handler.Handle(
            new BookingRecordDeletedDomainEventNotification
            {
                DomainEvent = domainEvent
            },
            CancellationToken.None);

        var outboxMessage = outboxRepository.Messages.Should().ContainSingle().Subject;
        var integrationEvent = MemoryPackSerializer.Deserialize<BookingCancelledIntegrationEvent>(outboxMessage.Payload);

        // Assert
        integrationEvent.Should().NotBeNull();
        integrationEvent.BookingRecord.StudentFullName.Should().Be("Volkov Nikita");

        integrationEvent.NotificationRecipients
            .Select(x => new { x.Email, x.UserId })
            .Should()
            .BeEquivalentTo(
            [
                new { Email = AllNotificationsEmail, UserId = notifyAllId },
                    new { Email = CancelledNotificationsEmail, UserId = notifyCancelledOnlyId }
            ]);
    }

    private static DateTime Utc(int year, int month, int day, int hour) =>
        new(year, month, day, hour, 0, 0, DateTimeKind.Utc);

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    private static Teacher CreateTeacher(Guid id, string firstName, string lastName) =>
        Teacher.Create(
            new TeacherId(id),
            firstName,
            lastName,
            null,
            new DateOnly(1990, 01, 01),
            CreatedAt,
            Now);

    private static Student CreateStudent(Guid id, GroupId groupId, string firstName, string lastName) =>
        Student.Create(
            new StudentId(id),
            groupId,
            firstName,
            lastName,
            null,
            new DateOnly(2004, 01, 01),
            CreatedAt,
            Now);

    private static Identity CreateIdentity(Guid id, string? email) =>
        new()
        {
            Id = id,
            UserName = email ?? $"user-{id:N}",
            Email = email
        };

    private static UserManager<Identity> CreateUserManager(params Identity[] users)
    {
        var manager = new Mock<UserManager<Identity>>(
            Mock.Of<IUserStore<Identity>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        manager.SetupGet(x => x.Users).Returns(users.AsQueryable());

        manager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => users.FirstOrDefault(x => x.Id.ToString() == id));

        return manager.Object;
    }

    private static IUnitOfWork CreateUnitOfWork(
        ITeacherRepository? teacherRepository = null,
        ISubjectRepository? subjectRepository = null,
        IGroupRepository? groupRepository = null,
        IStudentRepository? studentRepository = null,
        ISubmissionSlotRepository? submissionSlotRepository = null,
        IOutboxMessageRepository? outboxRepository = null)
    {
        var repositories = new Dictionary<Type, IRepository>();

        if (teacherRepository is not null)
            repositories[typeof(ITeacherRepository)] = teacherRepository;

        if (subjectRepository is not null)
            repositories[typeof(ISubjectRepository)] = subjectRepository;

        if (groupRepository is not null)
            repositories[typeof(IGroupRepository)] = groupRepository;

        if (studentRepository is not null)
            repositories[typeof(IStudentRepository)] = studentRepository;

        if (submissionSlotRepository is not null)
            repositories[typeof(ISubmissionSlotRepository)] = submissionSlotRepository;

        if (outboxRepository is not null)
            repositories[typeof(IOutboxMessageRepository)] = outboxRepository;

        return new FakeUnitOfWork(repositories);
    }

    private static ITeacherRepository CreateTeacherRepository(params Teacher[] teachers)
    {
        var mock = new Mock<ITeacherRepository>();

        mock.Setup(x => x.GetByIdAsync(It.IsAny<TeacherId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TeacherId id, CancellationToken _) => teachers.FirstOrDefault(x => x.Id == id));

        return mock.Object;
    }

    private static ISubjectRepository CreateSubjectRepository(params Subject[] subjects)
    {
        var mock = new Mock<ISubjectRepository>();

        mock.Setup(x => x.GetByIdAsync(It.IsAny<SubjectId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubjectId id, CancellationToken _) => subjects.FirstOrDefault(x => x.Id == id));

        return mock.Object;
    }

    private static IGroupRepository CreateGroupRepository(params Group[] groups)
    {
        var mock = new Mock<IGroupRepository>();

        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<GroupId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<GroupId> ids, CancellationToken _) => groups.Where(x => ids.Contains(x.Id)).ToArray());

        return mock.Object;
    }

    private static IStudentRepository CreateStudentRepository(params (Student Student, GroupId GroupId)[] students)
    {
        var mock = new Mock<IStudentRepository>();

        mock.Setup(x => x.GetByIdAsync(It.IsAny<StudentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentId id, CancellationToken _) => students.FirstOrDefault(x => x.Student.Id == id).Student);

        mock.Setup(x => x.GetByGroupIdAsync(It.IsAny<GroupId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupId id, CancellationToken _) => students.Where(x => x.GroupId == id).Select(x => x.Student).ToArray());

        mock.Setup(x => x.GetByGroupIdsAsync(It.IsAny<IEnumerable<GroupId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<GroupId> ids, CancellationToken _) =>
                students.Where(x => ids.Contains(x.GroupId)).Select(x => x.Student).ToArray());

        return mock.Object;
    }

    private static ISubmissionSlotRepository CreateSubmissionSlotRepository(params SubmissionSlot[] slots)
    {
        var mock = new Mock<ISubmissionSlotRepository>();

        mock.Setup(x => x.GetByIdAsync(It.IsAny<SubmissionSlotId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SubmissionSlotId id, CancellationToken _) => slots.FirstOrDefault(x => x.Id == id));

        return mock.Object;
    }

    private sealed class FakeUnitOfWork(
        IReadOnlyDictionary<Type, IRepository> repositories,
        IReadOnlyDictionary<Type, IQueryService>? queryServices = null) : IUnitOfWork
    {
        private static readonly IReadOnlyDictionary<Type, IQueryService> EmptyQueryServices =
            new Dictionary<Type, IQueryService>();

        public T GetRepository<T>() where T : IRepository =>
            (T)repositories[typeof(T)];

        public T GetQueryService<T>() where T : IQueryService =>
            (T)(queryServices ?? EmptyQueryServices)[typeof(T)];

        public Task BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }

    private sealed class FakeOutboxMessageRepository : IOutboxMessageRepository
    {
        public List<OutboxMessage> Messages { get; } = [];

        public Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OutboxMessage>>(Messages);

        public Task MarkAsProcessedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AddErrorAsync(Guid id, string error, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteProcessedAsync(DateTime beforeDate, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
