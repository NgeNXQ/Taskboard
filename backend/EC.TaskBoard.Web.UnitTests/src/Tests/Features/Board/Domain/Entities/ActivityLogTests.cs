using System;
using Xunit;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Entities;

public sealed class ActivityLogTests
{
    [Fact]
    public void Create_WithAllValues_SetsProperties()
    {
        var cardId = Guid.NewGuid();
        var kind = ActivityLogKind.CardUpdatedName;
        var current = "New Name";
        var previous = "Old Name";

        var log = ActivityLog.Create(cardId, kind, current, previous);

        Assert.Equal(kind, log.Kind);
        Assert.Equal(cardId, log.CardId);
        Assert.Equal(current, log.Current);
        Assert.Equal(previous, log.Previous);
    }

    [Fact]
    public void Create_WithNullCurrentAndPrevious_SetsNulls()
    {
        var cardId = Guid.NewGuid();

        var log = ActivityLog.Create(cardId, ActivityLogKind.CardCreated, null, null);

        Assert.Null(log.Current);
        Assert.Null(log.Previous);
    }

    [Fact]
    public void Create_ActivityLogIsTransient()
    {
        var log = ActivityLog.Create(Guid.NewGuid(), ActivityLogKind.CardCreated, "v", null);

        Assert.True(log.IsTransient);
    }

    [Theory]
    [MemberData(nameof(AllActivityLogKinds))]
    public void Create_AllKinds_SetsKindCorrectly(object kindObj)
    {
        var kind = (ActivityLogKind)kindObj;
        var log = ActivityLog.Create(Guid.NewGuid(), kind, "current", "previous");

        Assert.Equal(kind, log.Kind);
    }

    public static TheoryData<object> AllActivityLogKinds => new()
    {
        ActivityLogKind.CardCreated,
        ActivityLogKind.CardUpdatedName,
        ActivityLogKind.CardUpdatedList,
        ActivityLogKind.CardUpdatedDueDate,
        ActivityLogKind.CardUpdatedPriority,
        ActivityLogKind.CardUpdatedDescription,
        ActivityLogKind.CardDeleted,
    };
}
