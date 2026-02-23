using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

internal interface ICardService
{
    Task<CardResult> CreateAsync(CardCreateParams parameters, CancellationToken token);

    Task<CardResult> ReadByIdAsync(Guid id, CancellationToken token);
    Task<IEnumerable<CardResult>> ReadAllAsync(CancellationToken token);

    Task<CardResult> UpdatePartiallyAsync(CardUpdatePartialParams parameters, CancellationToken token);

    Task DeleteAsync(Guid id, CancellationToken token);
}
