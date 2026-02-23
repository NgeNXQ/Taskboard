using System;
using EC.TaskBoard.Web.Common.Foundation.Domain;
using EC.TaskBoard.Web.Features.Board.Domain.Enums;

namespace EC.TaskBoard.Web.Features.Board.Domain.Events;

internal sealed record CardAttributeChangedEvent(
    Guid CardId,
    ActivityLogKind Kind,
    string Current,
    string Previous
) : IDomainEvent;
