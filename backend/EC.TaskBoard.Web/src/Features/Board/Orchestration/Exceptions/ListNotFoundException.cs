using System;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Exceptions;

internal sealed class ListNotFoundException : EntryNotFoundException
{
    internal ListNotFoundException(Guid listId)
        : base(nameof(List), listId.ToString())
    {
    }
}
