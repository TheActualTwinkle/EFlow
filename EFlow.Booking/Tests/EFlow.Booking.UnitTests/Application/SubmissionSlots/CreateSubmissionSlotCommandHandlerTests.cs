using EFlow.Booking.Application.SubmissionSlots.Commands;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.UnitTests.Common;
using EFlow.Common.Infrastructure;
using FluentAssertions;
using Moq;

namespace EFlow.Booking.UnitTests.Application.SubmissionSlots;

public class CreateSubmissionSlotCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUseClockUtcNow_ForCreatedDomainEventTimestamp()
    {
        // Arrange
        var now = new DateTime(2026, 04, 18, 12, 0, 0, DateTimeKind.Utc);
        var systemClock = new FakeSystemClock(now);

        var repositoryMock = new Mock<ISubmissionSlotRepository>();
        SubmissionSlot? createdSlot = null;

        repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<SubmissionSlot>(), It.IsAny<CancellationToken>()))
            .Callback<SubmissionSlot, CancellationToken>((slot, _) => createdSlot = slot)
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.GetRepository<ISubmissionSlotRepository>())
            .Returns(repositoryMock.Object);

        var handler = new CreateSubmissionSlotCommandHandler(unitOfWorkMock.Object, systemClock);

        var command = new CreateSubmissionSlotCommand
        {
            SubjectId = Guid.NewGuid(),
            StartTime = now.AddDays(1),
            EndTime = now.AddDays(1).AddHours(1),
            MaxStudents = 10,
            AllowAllGroups = true,
            Location = "Room 101"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdSlot.Should().NotBeNull();

        var createdEvent = createdSlot!.DomainEvents
            .OfType<SubmissionSlotCreatedDomainEvent>()
            .Single();

        createdEvent.CreatedAt.Should().Be(now);

        repositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<SubmissionSlot>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

