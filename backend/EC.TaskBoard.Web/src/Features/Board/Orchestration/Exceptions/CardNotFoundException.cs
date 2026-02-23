using System;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;

internal sealed class CardNotFoundException : EntryNotFoundException
{
    internal CardNotFoundException(Guid cardId)
        : base(nameof(Card), cardId.ToString())
    {
    }
}
