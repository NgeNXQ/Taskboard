using System.Threading;
using System.Threading.Tasks;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Handlers;
using EC.TaskBoard.Web.Features.Board.Domain.Events;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Events;

internal sealed class CardAttributeChangedEventHandler
    : IDomainEventHandler<CardAttributeChangedEvent>
{
    private readonly IActivityLogService _activityLogService;

    public CardAttributeChangedEventHandler(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    public async Task Handle(CardAttributeChangedEvent domainEvent, CancellationToken token)
    {
        var activityLog = new ActivityLogCreateParams(
            domainEvent.CardId,
            domainEvent.Kind,
            domainEvent.Current,
            domainEvent.Previous
        );

        await _activityLogService.CreateAsync(activityLog, token);
    }
}
