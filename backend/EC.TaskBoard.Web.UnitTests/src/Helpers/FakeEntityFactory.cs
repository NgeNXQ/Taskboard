using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.UnitTests.Fakes;

namespace EC.TaskBoard.Web.UnitTests.Helpers;

internal sealed class FakeEntityFactory
{
    internal FakeEntity InstantiateTransient()
    {
        return new FakeEntity();
    }

    internal FakeEntity InstantiateNonTransient(Guid id)
    {
        var entity = new FakeEntity();

        typeof(Entity<Guid>)
            .GetProperty(nameof(entity.Id))!
            .SetValue(entity, id);

        return entity;
    }
}
