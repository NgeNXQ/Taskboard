using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Events;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Events;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Tests.Features.Board.Orchestration.Handlers;

public sealed class CardAttributeChangedEventHandlerTests
{
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly CardAttributeChangedEventHandler _sut;

    public CardAttributeChangedEventHandlerTests()
    {
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _sut = new CardAttributeChangedEventHandler(_activityLogServiceMock.Object);
    }

    [Fact]
    public async Task Handle_CallsActivityLogServiceCreateWithCorrectParams()
    {
        var cardId = Guid.NewGuid();
        var domainEvent = new CardAttributeChangedEvent(
            cardId,
            ActivityLogKind.CardUpdatedName,
            "New Name",
            "Old Name"
        );

        await _sut.Handle(domainEvent, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(
                It.Is<ActivityLogCreateParams>(p =>
                    p.CardId == cardId &&
                    p.Kind == ActivityLogKind.CardUpdatedName &&
                    p.Current == "New Name" &&
                    p.Previous == "Old Name"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Theory]
    [MemberData(nameof(AllActivityLogKinds))]
    public async Task Handle_AllKinds_CallsActivityLogService(object kindObj)
    {
        var kind = (ActivityLogKind)kindObj;
        var domainEvent = new CardAttributeChangedEvent(
            Guid.NewGuid(),
            kind,
            "current",
            "previous"
        );

        await _sut.Handle(domainEvent, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(
                It.Is<ActivityLogCreateParams>(p => p.Kind == kind),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    public static TheoryData<object> AllActivityLogKinds => new()
    {
        ActivityLogKind.CardCreated,
        ActivityLogKind.CardUpdatedName,
        ActivityLogKind.CardUpdatedList,
        ActivityLogKind.CardUpdatedDueDate,
        ActivityLogKind.CardUpdatedPriority,
        ActivityLogKind.CardUpdatedDescription,
        ActivityLogKind.CardDeleted,
    };
}
