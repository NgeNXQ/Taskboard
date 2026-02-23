using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface IListService
{
    Task<ListResult> CreateAsync(ListCreateParams parameters, CancellationToken token);

    Task<IEnumerable<ListResult>> ReadAllAsync(CancellationToken token);

    Task<ListResult> UpdateAsync(ListUpdateParams parameters, CancellationToken token);

    Task DeleteAsync(Guid id, CancellationToken token);

    Task<bool> DoesExistWitIdAsync(Guid id, CancellationToken token);
}
