using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Controllers;

[ApiController]
[Route("api/rest/[controller]")]
internal sealed class ActivityLogController : Controller
{
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<ActivityLogController> _logger;

    public ActivityLogController(
        ILogger<ActivityLogController> logger,
        IActivityLogService activityLogService
    )
    {
        _logger = logger;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ActivityLogResult>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ActivityLogResult>>> ReadAllPaginated(
        [FromQuery] int offset,
        [FromQuery] int limit,
        CancellationToken token
    )
    {
        _logger.LogInformation("GET activity-log ALL offset={Offset} limit={Limit}", offset, limit);

        var result = await _activityLogService.ReadAllPaginatedAsync(offset, limit, token);

        _logger.LogInformation("READ activity-log ALL total={Total}", result.Meta.Total);

        return Ok(result);
    }

    [HttpGet("card/{cardId}")]
    [ProducesResponseType<IEnumerable<ActivityLogResult>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ActivityLogResult>>> ReadAllByCardId(
        [FromRoute] Guid cardId,
        CancellationToken token
    )
    {
        _logger.LogInformation("GET activity-log id={CardId}", cardId);

        var entries = await _activityLogService.ReadAllByCardIdAsync(cardId, token);

        _logger.LogInformation("READ activity-log id={CardId}", cardId);

        return Ok(entries);
    }
}
