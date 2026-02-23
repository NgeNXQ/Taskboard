using System;
using Xunit;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Events;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.UnitTests.Helpers;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Entities;

public sealed class CardTests
{
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 2000;

    private readonly DefaultCardFactory _defaultCardFactory;

    public CardTests()
    {
        _defaultCardFactory = new DefaultCardFactory();
    }

    [Fact]
    public void Create_WithValidArguments_ReturnsCard()
    {
        var listId = Guid.NewGuid();
        var dueDate = DateTimeOffset.UtcNow.AddDays(7);

        var card = Card.Create(
            listId,
            "Name",
            CardPriority.Medium,
            "Desc",
            dueDate,
            MaxNameLength,
            MaxDescriptionLength
        );

        Assert.NotNull(card);
        Assert.Equal(listId, card.ListId);
        Assert.Equal("Name", card.Name);
        Assert.Equal(CardPriority.Medium, card.Priority);
        Assert.Equal("Desc", card.Description);
        Assert.Equal(dueDate, card.DueDate);
    }

    [Fact]
    public void Create_NewCard_HasNoEvents()
    {
        var card = _defaultCardFactory.Instantiate();

        Assert.Empty(card.Events);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsDomainException(string? name)
    {
        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                name!,
                CardPriority.Medium,
                "Desc",
                DateTimeOffset.UtcNow.AddDays(7),
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Fact]
    public void Create_NameExceedsMaxLength_ThrowsDomainException()
    {
        var name = new string('a', MaxNameLength + 1);

        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                name,
                CardPriority.Medium,
                "Desc",
                DateTimeOffset.UtcNow.AddDays(7),
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Fact]
    public void Create_PastDueDate_ThrowsDomainException()
    {
        var past = DateTimeOffset.UtcNow.AddDays(-1);

        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                "Name",
                CardPriority.Medium,
                "Desc",
                past,
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Fact]
    public void Create_NonUtcDueDate_ThrowsDomainException()
    {
        TimeSpan offset = TimeSpan.FromHours(2);

        var nonUtdDate = new DateTimeOffset(
            DateTime.Now.AddYears(1).Year,
            DateTime.Now.Month,
            DateTime.Now.Day,
            12, 0, 0,
            offset
        );

        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                "Name",
                CardPriority.Medium,
                "Desc",
                nonUtdDate,
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyDescription_ThrowsDomainException(string? description)
    {
        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                "Name",
                CardPriority.Medium,
                description!,
                DateTimeOffset.UtcNow.AddDays(7),
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Fact]
    public void Create_DescriptionExceedsMaxLength_ThrowsDomainException()
    {
        var description = new string('d', MaxDescriptionLength + 1);

        Assert.Throws<DomainException>(() =>
            Card.Create(
                Guid.NewGuid(),
                "Name",
                CardPriority.Medium,
                description,
                DateTimeOffset.UtcNow.AddDays(7),
                MaxNameLength,
                MaxDescriptionLength
            )
        );
    }

    [Fact]
    public void ChangeName_DifferentName_UpdatesName()
    {
        var card = _defaultCardFactory.Instantiate(name: "Old Name");

        card.ChangeName("New Name", MaxNameLength);

        Assert.Equal("New Name", card.Name);
    }

    [Fact]
    public void ChangeName_DifferentName_RaisesCardAttributeChangedEvent()
    {
        var card = _defaultCardFactory.Instantiate(name: "Old Name");

        card.ChangeName("New Name", MaxNameLength);

        var evt = Assert.Single(card.Events);
        var changedEvent = Assert.IsType<CardAttributeChangedEvent>(evt);
        Assert.Equal(ActivityLogKind.CardUpdatedName, changedEvent.Kind);
        Assert.Equal("New Name", changedEvent.Current);
        Assert.Equal("Old Name", changedEvent.Previous);
    }

    [Fact]
    public void ChangeName_SameName_DoesNotRaiseEvent()
    {
        var card = _defaultCardFactory.Instantiate(name: "Same Name");

        card.ChangeName("Same Name", MaxNameLength);

        Assert.Empty(card.Events);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeName_InvalidName_ThrowsDomainException(string? name)
    {
        var card = _defaultCardFactory.Instantiate();

        Assert.Throws<DomainException>(() => card.ChangeName(name!, MaxNameLength));
    }

    [Fact]
    public void ChangeName_NameExceedsMaxLength_ThrowsDomainException()
    {
        var card = _defaultCardFactory.Instantiate();
        var longName = new string('a', MaxNameLength + 1);

        Assert.Throws<DomainException>(() => card.ChangeName(longName, MaxNameLength));
    }

    [Fact]
    public void ChangePriority_DifferentPriority_UpdatesPriority()
    {
        var card = _defaultCardFactory.Instantiate(priority: CardPriority.Lowest);

        card.ChangePriority(CardPriority.Highest);

        Assert.Equal(CardPriority.Highest, card.Priority);
    }

    [Fact]
    public void ChangePriority_DifferentPriority_RaisesCardAttributeChangedEvent()
    {
        var card = _defaultCardFactory.Instantiate(priority: CardPriority.Lowest);

        card.ChangePriority(CardPriority.Highest);

        var evt = Assert.Single(card.Events);
        var changedEvent = Assert.IsType<CardAttributeChangedEvent>(evt);
        Assert.Equal(ActivityLogKind.CardUpdatedPriority, changedEvent.Kind);
        Assert.Equal(CardPriority.Highest.ToString(), changedEvent.Current);
        Assert.Equal(CardPriority.Lowest.ToString(), changedEvent.Previous);
    }

    [Fact]
    public void ChangePriority_SamePriority_DoesNotRaiseEvent()
    {
        var card = _defaultCardFactory.Instantiate(priority: CardPriority.Medium);

        card.ChangePriority(CardPriority.Medium);

        Assert.Empty(card.Events);
    }

    [Fact]
    public void ChangeDueDate_DifferentFutureDate_UpdatesDueDate()
    {
        var card = _defaultCardFactory.Instantiate();
        var newDate = DateTimeOffset.UtcNow.AddDays(14);

        card.ChangeDueDate(newDate);

        Assert.Equal(newDate, card.DueDate);
    }

    [Fact]
    public void ChangeDueDate_DifferentDate_RaisesCardAttributeChangedEvent()
    {
        var oldDueDate = DateTimeOffset.UtcNow.AddDays(7);
        var card = _defaultCardFactory.Instantiate(dueDate: oldDueDate);
        var newDueDate = DateTimeOffset.UtcNow.AddDays(14);

        card.ChangeDueDate(newDueDate);

        var evt = Assert.Single(card.Events);
        var changedEvent = Assert.IsType<CardAttributeChangedEvent>(evt);
        Assert.Equal(ActivityLogKind.CardUpdatedDueDate, changedEvent.Kind);
    }

    [Fact]
    public void ChangeDueDate_SameDate_DoesNotRaiseEvent()
    {
        var dueDate = DateTimeOffset.UtcNow.AddDays(7);
        var card = _defaultCardFactory.Instantiate(dueDate: dueDate);

        card.ChangeDueDate(dueDate);

        Assert.Empty(card.Events);
    }

    [Fact]
    public void ChangeDueDate_PastDate_ThrowsDomainException()
    {
        var card = _defaultCardFactory.Instantiate();

        Assert.Throws<DomainException>(() =>
            card.ChangeDueDate(DateTimeOffset.UtcNow.AddDays(-1)));
    }

    [Fact]
    public void ChangeDueDate_NonUtcOffset_ThrowsDomainException()
    {
        var card = _defaultCardFactory.Instantiate();

        TimeSpan offset = TimeSpan.FromHours(2);

        var nonUtdDate = new DateTimeOffset(
            DateTime.Now.AddYears(1).Year,
            DateTime.Now.Month,
            DateTime.Now.Day,
            12, 0, 0,
            offset
        );

        Assert.Throws<DomainException>(() => card.ChangeDueDate(nonUtdDate));
    }

    [Fact]
    public void ChangeDescription_DifferentDescription_UpdatesDescription()
    {
        var card = _defaultCardFactory.Instantiate(description: "Old Description");

        card.ChangeDescription("New Description", MaxDescriptionLength);

        Assert.Equal("New Description", card.Description);
    }

    [Fact]
    public void ChangeDescription_DifferentDescription_RaisesCardAttributeChangedEvent()
    {
        var card = _defaultCardFactory.Instantiate(description: "Old");

        card.ChangeDescription("New", MaxDescriptionLength);

        var evt = Assert.Single(card.Events);
        var changedEvent = Assert.IsType<CardAttributeChangedEvent>(evt);
        Assert.Equal(ActivityLogKind.CardUpdatedDescription, changedEvent.Kind);
        Assert.Equal("New", changedEvent.Current);
        Assert.Equal("Old", changedEvent.Previous);
    }

    [Fact]
    public void ChangeDescription_SameDescription_DoesNotRaiseEvent()
    {
        var card = _defaultCardFactory.Instantiate(description: "Same");

        card.ChangeDescription("Same", MaxDescriptionLength);

        Assert.Empty(card.Events);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangeDescription_InvalidDescription_ThrowsDomainException(string? description)
    {
        var card = _defaultCardFactory.Instantiate();

        Assert.Throws<DomainException>(() =>
            card.ChangeDescription(description!, MaxDescriptionLength)
        );
    }

    [Fact]
    public void ChangeDescription_ExceedsMaxLength_ThrowsDomainException()
    {
        var card = _defaultCardFactory.Instantiate();

        var longDesc = new string('d', MaxDescriptionLength + 1);

        Assert.Throws<DomainException>(() =>
            card.ChangeDescription(longDesc, MaxDescriptionLength)
        );
    }

    [Fact]
    public void ChangeList_DifferentListId_UpdatesListId()
    {
        var oldListId = Guid.NewGuid();
        var card = _defaultCardFactory.Instantiate(listId: oldListId);
        var newListId = Guid.NewGuid();

        card.ChangeList(newListId);

        Assert.Equal(newListId, card.ListId);
    }

    [Fact]
    public void ChangeList_DifferentListId_RaisesCardAttributeChangedEvent()
    {
        var oldListId = Guid.NewGuid();
        var card = _defaultCardFactory.Instantiate(listId: oldListId);
        var newListId = Guid.NewGuid();

        card.ChangeList(newListId);

        var evt = Assert.Single(card.Events);
        var changedEvent = Assert.IsType<CardAttributeChangedEvent>(evt);
        Assert.Equal(ActivityLogKind.CardUpdatedList, changedEvent.Kind);
        Assert.Equal(newListId.ToString(), changedEvent.Current);
        Assert.Equal(oldListId.ToString(), changedEvent.Previous);
    }

    [Fact]
    public void ChangeList_SameListId_DoesNotRaiseEvent()
    {
        var listId = Guid.NewGuid();
        var card = _defaultCardFactory.Instantiate(listId: listId);

        card.ChangeList(listId);

        Assert.Empty(card.Events);
    }

    [Fact]
    public void ChangeX_MultipleChanges_AccumulateEvents()
    {
        var card = _defaultCardFactory.Instantiate(name: "Old", priority: CardPriority.Lowest);

        card.ChangeName("New", MaxNameLength);
        card.ChangePriority(CardPriority.Highest);

        Assert.Equal(2, card.Events.Count);
    }

    [Fact]
    public void ResetEvents_AfterMultipleChanges_ClearsAllEvents()
    {
        var card = _defaultCardFactory.Instantiate(name: "Old");
        card.ChangeName("New", MaxNameLength);
        card.ChangePriority(CardPriority.Highest);

        card.ResetEvents();

        Assert.Empty(card.Events);
    }
}
