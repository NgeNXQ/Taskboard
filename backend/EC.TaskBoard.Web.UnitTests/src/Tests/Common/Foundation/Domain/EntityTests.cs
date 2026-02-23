using System;
using System.Linq;
using Xunit;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.UnitTests.Fakes;
using EC.TaskBoard.Web.UnitTests.Helpers;

namespace EC.TaskBoard.Web.UnitTests.Tests.Common.Foundation.Domain;

file sealed class OtherEntity : Entity<Guid>
{
    internal static OtherEntity Instantiate(Guid id)
    {
        var entity = new OtherEntity();

        typeof(Entity<Guid>)
            .GetProperty(nameof(Id))!
            .SetValue(entity, id);

        return entity;
    }
}

public sealed class EntityTests
{
    private readonly FakeEntityFactory _fakeEntityFactory;

    public EntityTests()
    {
        _fakeEntityFactory = new FakeEntityFactory();
    }

    [Fact]
    public void IsTransient_IdIsDefault_ReturnsTrue()
    {
        var entity = _fakeEntityFactory.InstantiateTransient();

        Assert.True(entity.IsTransient);
    }

    [Fact]
    public void IsTransient_IdIsAssigned_ReturnsFalse()
    {
        var entity = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());

        Assert.False(entity.IsTransient);
    }

    [Fact]
    public void Events_NewEntity_InitiallyEmpty()
    {
        var entity = _fakeEntityFactory.InstantiateTransient();

        Assert.Empty(entity.Events);
    }

    [Fact]
    public void RaiseEvent_EmptyEvents_AddsEventToCollection()
    {
        var entity = _fakeEntityFactory.InstantiateTransient();
        var domainEvent = new FakeDomainEvent();

        entity.RaiseEvent(domainEvent);

        Assert.Single(entity.Events);
        Assert.Same(domainEvent, entity.Events.First());
    }

    [Fact]
    public void RaiseEvent_EventIsNull_ThrowsArgumentNullException()
    {
        var entity = _fakeEntityFactory.InstantiateTransient();

        Assert.Throws<ArgumentNullException>(() => entity.RaiseEvent(null!));
    }

    [Fact]
    public void ResetEvents_FilledEvents_ClearsAllEvents()
    {
        var entity = _fakeEntityFactory.InstantiateTransient();
        entity.RaiseEvent(new FakeDomainEvent());
        entity.RaiseEvent(new FakeDomainEvent());

        entity.ResetEvents();

        Assert.Empty(entity.Events);
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var entity = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_TwoEntitiesWithSameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var first = _fakeEntityFactory.InstantiateNonTransient(id);
        var second = _fakeEntityFactory.InstantiateNonTransient(id);

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_TwoEntitiesWithDifferentIds_ReturnsFalse()
    {
        var first = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());
        var second = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_TransientEntity_ReturnsFalse()
    {
        var first = _fakeEntityFactory.InstantiateTransient();
        var second = _fakeEntityFactory.InstantiateTransient();

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_NullEntity_ReturnsFalse()
    {
        var entity = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());

        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Equals_DifferentTypesWithSameId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var fake = _fakeEntityFactory.InstantiateNonTransient(id);
        var other = OtherEntity.Instantiate(id);

        Assert.False(fake.Equals(other));
    }

    [Fact]
    public void GetHashCode_TwoEntitiesWithSameId_ReturnSameHashCode()
    {
        var id = Guid.NewGuid();
        var first = _fakeEntityFactory.InstantiateNonTransient(id);
        var second = _fakeEntityFactory.InstantiateNonTransient(id);

        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_BothNull_ReturnsTrue()
    {
        FakeEntity? first = null;
        FakeEntity? second = null;

        Assert.True(first == second);
    }

    [Fact]
    public void EqualityOperator_OneNull_ReturnsFalse()
    {
        FakeEntity? first = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());
        FakeEntity? second = null;

        Assert.False(first == second);
        Assert.False(second == first);
    }

    [Fact]
    public void InequalityOperator_DifferentEntities_ReturnsTrue()
    {
        var first = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());
        var second = _fakeEntityFactory.InstantiateNonTransient(Guid.NewGuid());

        Assert.True(first != second);
    }
}
