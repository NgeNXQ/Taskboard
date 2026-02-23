using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;
using EC.TaskBoard.Web.Features.Board.Domain.Events;
using EC.TaskBoard.Web.Features.Board.Domain.Guards;

namespace EC.TaskBoard.Web.Features.Board.Domain.Entities;

internal sealed class Card : Entity<Guid>, IHasCreateTime, IHasUpdateTime, IHasDeleteTime
{
    internal static Card Create(
        Guid listId,
        string name,
        CardPriority priority,
        string description,
        DateTimeOffset dueDate,
        int maxNameLength,
        int maxDescriptionLength
    )
    {
        CardGuard.EnsureValidName(name, maxNameLength);
        CardGuard.EnsureValidDueDate(dueDate);
        CardGuard.EnsureValidDescription(description, maxDescriptionLength);

        return new Card()
        {
            ListId = listId,
            Name = name,
            DueDate = dueDate,
            Priority = priority,
            Description = description,
        };
    }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid ListId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CardPriority Priority { get; private set; }
    public DateTimeOffset DueDate { get; private set; }

    internal void ChangeList(Guid id)
    {
        if (id != ListId)
        {
            RaiseEvent(new CardAttributeChangedEvent(
                Id, ActivityLogKind.CardUpdatedList, id.ToString(), ListId.ToString()
            ));
        }

        ListId = id;
    }

    internal void ChangeName(string name, int maxLength)
    {
        CardGuard.EnsureValidName(name, maxLength);

        if (name != Name)
        {
            RaiseEvent(new CardAttributeChangedEvent(
                Id, ActivityLogKind.CardUpdatedName, name, Name
            ));
        }

        Name = name;
    }

    internal void ChangePriority(CardPriority priority)
    {
        if (priority != Priority)
        {
            RaiseEvent(new CardAttributeChangedEvent(
                Id, ActivityLogKind.CardUpdatedPriority, priority.ToString(), Priority.ToString()
            ));
        }

        Priority = priority;
    }

    internal void ChangeDueDate(DateTimeOffset dueDate)
    {
        CardGuard.EnsureValidDueDate(dueDate);

        if (dueDate != DueDate)
        {
            RaiseEvent(new CardAttributeChangedEvent(
                Id, ActivityLogKind.CardUpdatedDueDate, dueDate.ToString(), DueDate.ToString()
            ));
        }

        DueDate = dueDate;
    }

    internal void ChangeDescription(string description, int maxLength)
    {
        CardGuard.EnsureValidDescription(description, maxLength);

        if (description != Description)
        {
            RaiseEvent(new CardAttributeChangedEvent(
                Id, ActivityLogKind.CardUpdatedDescription, description, Description
            ));
        }

        Description = description;
    }
}
