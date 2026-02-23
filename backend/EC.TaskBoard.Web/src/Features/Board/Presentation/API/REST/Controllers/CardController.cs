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
internal sealed class CardController : Controller
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardController> _logger;

    public CardController(
        ILogger<CardController> logger,
        ICardService cardService
    )
    {
        _logger = logger;
        _cardService = cardService;
    }

    [HttpPost]
    [ProducesResponseType<CardResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResult>> CreateCard(
        [FromBody] CardCreateParams payload,
        CancellationToken token
    )
    {
        _logger.LogInformation("POST card {@CardCreateRequest}", payload);

        var card = await _cardService.CreateAsync(payload, token);

        _logger.LogInformation("CREATE card {CardId}", card.Id);

        return CreatedAtAction(
            nameof(ReadByIdCard),
            new { id = card.Id },
            card
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType<CardResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResult>> ReadByIdCard(
        [FromRoute] Guid id,
        CancellationToken token
    )
    {
        _logger.LogInformation("GET card {CardId}", id);

        var card = await _cardService.ReadByIdAsync(id, token);

        _logger.LogInformation("READ card {CardId}", id);

        return Ok(card);
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<CardResult>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CardResult>>> ReadAllCards(
        CancellationToken token
    )
    {
        _logger.LogInformation("GET card ALL");

        var cards = await _cardService.ReadAllAsync(token);

        _logger.LogInformation("READ card ALL");

        return Ok(cards);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType<CardResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResult>> UpdatePartiallyCard(
        [FromRoute] Guid id,
        [FromBody] CardUpdateRequest request,
        CancellationToken token
    )
    {
        _logger.LogInformation("PATCH card {CardId}", id);

        var parameters = new CardUpdatePartialParams(
            id,
            request.ListId,
            request.Name,
            request.Description,
            request.Priority,
            request.DueDate
        );

        var card = await _cardService.UpdatePartiallyAsync(parameters, token);

        _logger.LogInformation("UPDATE card {CardId}", id);

        return Ok(card);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteByIdCard(
        [FromRoute] Guid id,
        CancellationToken token
    )
    {
        _logger.LogInformation("DELETE card {CardId}", id);

        await _cardService.DeleteAsync(id, token);

        _logger.LogInformation("DELETE card {CardId}", id);

        return NoContent();
    }
}
