using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Orchestration.Notifications;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Notifications;

internal sealed class CardCreatedNotificationHandler
    : INotificationHandler<CardCreatedNotification>
{
    private readonly IActivityLogService _activityLogService;

    public CardCreatedNotificationHandler(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    public async Task Handle(CardCreatedNotification notification, CancellationToken token)
    {
        var activityLog = new ActivityLogCreateParams(
            notification.CardId,
            ActivityLogKind.CardCreated,
            notification.Name,
            null
        );

        await _activityLogService.CreateAsync(activityLog, token);
    }
}
