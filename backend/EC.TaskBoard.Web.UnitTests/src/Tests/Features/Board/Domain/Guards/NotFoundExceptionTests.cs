using System;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;
using Xunit;

namespace EC.TaskBoard.Web.Tests.Features.Board.Domain.Guards;

public sealed class CardNotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsResourceIdentityToCard()
    {
        var cardId = Guid.NewGuid();

        var exception = new CardNotFoundException(cardId);

        Assert.Equal("Card", exception.ResourceIdentity);
    }

    [Fact]
    public void Constructor_SetsResourceIdentifierToCardIdString()
    {
        var cardId = Guid.NewGuid();

        var exception = new CardNotFoundException(cardId);

        Assert.Equal(cardId.ToString(), exception.ResourceIdentifier);
    }

    [Fact]
    public void CardNotFoundException_InheritsEntryNotFoundException()
    {
        var exception = new CardNotFoundException(Guid.NewGuid());

        Assert.IsAssignableFrom<EntryNotFoundException>(exception);
    }
}

public sealed class ListNotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsResourceIdentityToList()
    {
        var listId = Guid.NewGuid();

        var exception = new ListNotFoundException(listId);

        Assert.Equal("List", exception.ResourceIdentity);
    }

    [Fact]
    public void Constructor_SetsResourceIdentifierToListIdString()
    {
        var listId = Guid.NewGuid();

        var exception = new ListNotFoundException(listId);

        Assert.Equal(listId.ToString(), exception.ResourceIdentifier);
    }

    [Fact]
    public void ListNotFoundException_InheritsEntryNotFoundException()
    {
        var exception = new ListNotFoundException(Guid.NewGuid());

        Assert.IsAssignableFrom<EntryNotFoundException>(exception);
    }
}
