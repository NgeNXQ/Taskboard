using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Schemas;
using EC.TaskBoard.Web.Features.Board.Orchestration.Models;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Controllers;

[ApiController]
[Route("api/rest/[controller]")]
internal sealed class ListController : Controller
{
    private readonly IListService _listService;
    private readonly ILogger<ListController> _logger;

    public ListController(
        ILogger<ListController> logger,
        IListService listService
    )
    {
        _logger = logger;
        _listService = listService;
    }

    [HttpPost]
    [ProducesResponseType<ListResult>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ListResult>> CreateList(
        [FromBody] ListCreateParams payload,
        CancellationToken token
    )
    {
        _logger.LogInformation("POST list {@ListCreateRequest}", payload);

        var list = await _listService.CreateAsync(payload, token);

        _logger.LogInformation("CREATE list {ListId}", list.Id);

        return CreatedAtAction(
            nameof(ReadAllLists),
            list
        );
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ListResult>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ListResult>>> ReadAllLists(
        CancellationToken token
    )
    {
        _logger.LogInformation("GET list ALL");

        var lists = await _listService.ReadAllAsync(token);

        _logger.LogInformation("READ list ALL");

        return Ok(lists);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<ListResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResult>> UpdateList(
        [FromRoute] Guid id,
        [FromBody] ListUpdateRequest request,
        CancellationToken token
    )
    {
        _logger.LogInformation("PUT list {ListId}", id);

        var parameters = new ListUpdateParams(id, request.Name);

        var list = await _listService.UpdateAsync(parameters, token);

        _logger.LogInformation("UPDATE list {ListId}", id);

        return Ok(list);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteByIdList(
        [FromRoute] Guid id,
        CancellationToken token
    )
    {
        _logger.LogInformation("DELETE list {ListId}", id);

        await _listService.DeleteAsync(id, token);

        _logger.LogInformation("DELETED list {ListId}", id);

        return NoContent();
    }
}
