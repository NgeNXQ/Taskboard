using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Notifications;

internal sealed class CardDeletedNotificationHandler
    : INotificationHandler<CardDeletedNotification>
{
    private readonly IActivityLogService _activityLogService;

    public CardDeletedNotificationHandler(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    public async Task Handle(CardDeletedNotification notification, CancellationToken token)
    {
        var activityLog = new ActivityLogCreateParams(
            notification.CardId,
            ActivityLogKind.CardDeleted,
            null,
            notification.Name
        );

        await _activityLogService.CreateAsync(activityLog, token);
    }
}
