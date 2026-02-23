using System;
using System.Collections.Generic;

namespace EC.TaskBoard.Web.Common.Foundation.Domain;

public abstract class Entity<TId> : IEntity, IEquatable<Entity<TId>>
    where TId : notnull
{
    private readonly List<IDomainEvent> _events = new();

    protected Entity()
    {
        Id = default!;
        Events = _events.AsReadOnly();
    }

    public TId Id { get; private init; }

    public IReadOnlyList<IDomainEvent> Events { get; }

    public bool IsTransient
    {
        get => EqualityComparer<TId>.Default.Equals(Id, default!);
    }

    public override sealed bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public override sealed int GetHashCode()
    {
        if (IsTransient)
            return base.GetHashCode();

        return HashCode.Combine(GetType(), Id);
    }

    public void ResetEvents()
    {
        _events.Clear();
    }

    public void RaiseEvent(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent ?? throw new ArgumentNullException(nameof(domainEvent)));
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (IsTransient || other.IsTransient)
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
