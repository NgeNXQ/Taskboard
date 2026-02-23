using System.Collections.Generic;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public interface IEntity
{
    bool IsTransient { get; }

    IReadOnlyList<IDomainEvent> Events { get; }

    void ResetEvents();

    void RaiseEvent(IDomainEvent domainEvent);
}
