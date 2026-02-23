using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Notifications;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

namespace EC.TaskBoard.Web.Tests.Features.Board.Orchestration.Handlers;

public sealed class CardCreatedNotificationHandlerTests
{
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly CardCreatedNotificationHandler _sut;

    public CardCreatedNotificationHandlerTests()
    {
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _sut = new CardCreatedNotificationHandler(_activityLogServiceMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesActivityLogWithCardCreatedKind()
    {
        var cardId = Guid.NewGuid();
        const string cardName = "My Card";
        var notification = new CardCreatedNotification(cardId, cardName);

        await _sut.Handle(notification, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(
                It.Is<ActivityLogCreateParams>(p =>
                    p.CardId == cardId &&
                    p.Kind == ActivityLogKind.CardCreated &&
                    p.Current == cardName &&
                    p.Previous == null
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CallsActivityLogServiceExactlyOnce()
    {
        var notification = new CardCreatedNotification(Guid.NewGuid(), "Name");

        await _sut.Handle(notification, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<ActivityLogCreateParams>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
