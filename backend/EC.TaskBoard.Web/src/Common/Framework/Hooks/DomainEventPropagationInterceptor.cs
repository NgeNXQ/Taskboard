using System;
using System.Linq;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Common.Framework.Hooks;

public sealed class DomainEventPropagationInterceptor : SaveChangesInterceptor
{
    private const int MaxPropagationDepth = 10;

    private readonly IPublisher _publisher;

    public DomainEventPropagationInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken
    )
    {
        await PropagateEventsAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async ValueTask PropagateEventsAsync(DbContext? context, CancellationToken token)
    {
        if (context is null)
            return;

        int depth = 0;

        while (true)
        {
            if (depth++ > MaxPropagationDepth)
            {
                throw new StackOverflowException(
                    "Maximum domain event propagation depth reached due to the circular events."
                );
            }

            var entries = FilterEntities(context);

            if (entries.Count == 0)
                break;

            foreach (var entity in entries)
            {
                var pool = ArrayPool<IDomainEvent>.Shared;
                var count = entity.Events.Count;
                var buffer = pool.Rent(count);

                try
                {
                    var events = entity.Events;

                    for (int i = 0; i < count; ++i)
                        buffer[i] = events[i];

                    entity.ResetEvents();

                    for (int i = 0; i < count; ++i)
                        await _publisher.Publish(buffer[i], token);
                }
                finally
                {
                    pool.Return(buffer, clearArray: true);
                }
            }
        }
    }

    private IList<IEntity> FilterEntities(DbContext context)
    {
        return context.ChangeTracker
            .Entries<IEntity>()
            .Select(entity => entity.Entity)
            .Where(entity => entity.Events.Any())
            .ToList();
    }
}
