using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Domain.Entities;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Features.Board.Orchestration.Services;

internal sealed class ActivityLogService : IActivityLogService
{
    private readonly IMapper _mapper;
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogService(
        IMapper mapper,
        IActivityLogRepository activityLogRepository
    )
    {
        _mapper = mapper;
        _activityLogRepository = activityLogRepository;
    }

    public async Task CreateAsync(ActivityLogCreateParams parameters, CancellationToken token)
    {
        var activityLog = ActivityLog.Create(
            parameters.CardId,
            parameters.Kind,
            parameters.Current,
            parameters.Previous
        );

        _activityLogRepository.Create(activityLog);
    }

    public async Task<IEnumerable<ActivityLogResult>> ReadAllByCardIdAsync(
        Guid id,
        CancellationToken token
    )
    {
        var activityLogs = await _activityLogRepository.ReadAllByCardIdAsync(id, token);

        return _mapper.Map<IEnumerable<ActivityLogResult>>(activityLogs);
    }

    public async Task<SimplePaginatedResult<ActivityLogResult>> ReadAllPaginatedAsync(
        int offset,
        int limit,
        CancellationToken token
    )
    {
        var (data, total) = await _activityLogRepository.ReadAllPaginatedAsync(
            offset,
            limit,
            token
        );

        var mapped = _mapper.Map<IEnumerable<ActivityLogResult>>(data);

        var meta = new SimplePaginationMeta(offset, limit, total, offset + limit < total);

        return new SimplePaginatedResult<ActivityLogResult>(mapped, meta);
    }
}
