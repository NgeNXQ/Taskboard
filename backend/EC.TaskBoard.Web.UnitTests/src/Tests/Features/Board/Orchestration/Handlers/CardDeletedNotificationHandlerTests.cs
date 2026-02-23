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

public sealed class CardDeletedNotificationHandlerTests
{
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly CardDeletedNotificationHandler _sut;

    public CardDeletedNotificationHandlerTests()
    {
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _sut = new CardDeletedNotificationHandler(_activityLogServiceMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesActivityLogWithCardDeletedKind()
    {
        var cardId = Guid.NewGuid();
        const string cardName = "Deleted Card";
        var notification = new CardDeletedNotification(cardId, cardName);

        await _sut.Handle(notification, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(
                It.Is<ActivityLogCreateParams>(p =>
                    p.CardId == cardId &&
                    p.Kind == ActivityLogKind.CardDeleted &&
                    p.Current == null &&
                    p.Previous == cardName
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CallsActivityLogServiceExactlyOnce()
    {
        var notification = new CardDeletedNotification(Guid.NewGuid(), "Name");

        await _sut.Handle(notification, CancellationToken.None);

        _activityLogServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<ActivityLogCreateParams>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
