using System;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.UnitTests.Helpers;

internal sealed class DefaultCardFactory
{
    internal Card Instantiate(
        Guid? listId = null,
        string name = "Name",
        string description = "Desc",
        CardPriority priority = CardPriority.Medium,
        DateTimeOffset? dueDate = null,
        int maxNameLength = 200,
        int maxDescriptionLength = 1000
    )
    {
        var validFutureDate = DateTimeOffset.UtcNow.AddDays(7);

        return Card.Create(
            listId ?? Guid.NewGuid(),
            name,
            priority,
            description,
            dueDate ?? validFutureDate,
            maxNameLength,
            maxDescriptionLength
        );
    }
}
