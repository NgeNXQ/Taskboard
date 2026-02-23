using MediatR;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Common.Foundation.Orchestration.Handlers;

public interface IDomainEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
