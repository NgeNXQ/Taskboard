using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Common.Framework.Hooks;

public sealed class EntityTimestampPopulationInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken
    )
    {
        PopulateTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void PopulateTimestamps(DbContext? context)
    {
        if (context is null)
            return;

        var timestamp = DateTime.UtcNow;

        var entries = context.ChangeTracker
            .Entries<IEntity>()
            .Where(entry =>
                entry.State is EntityState.Added or EntityState.Deleted or EntityState.Modified
            );

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added when entry is IHasCreateTime caseEntry:
                    caseEntry.CreatedAt = timestamp;
                    (caseEntry as IHasUpdateTime)?.UpdatedAt = timestamp;
                    break;
                case EntityState.Deleted when entry is IHasDeleteTime caseEntry:
                    entry.State = EntityState.Modified;
                    caseEntry.DeletedAt = timestamp;
                    (caseEntry as IHasUpdateTime)?.UpdatedAt = timestamp;
                    break;
                case EntityState.Modified when entry is IHasUpdateTime caseEntry:
                    caseEntry.UpdatedAt = timestamp;
                    break;
            }
        }
    }
}
