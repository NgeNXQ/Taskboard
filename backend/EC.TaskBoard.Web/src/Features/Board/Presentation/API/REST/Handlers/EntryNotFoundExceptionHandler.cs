using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Exceptions;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Handlers;

internal sealed class EntryNotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<EntryNotFoundExceptionHandler> _logger;

    public EntryNotFoundExceptionHandler(ILogger<EntryNotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken token
    )
    {
        if (exception is not EntryNotFoundException ex)
            return false;

        if (context.Response.HasStarted)
            return false;

        _logger.LogWarning(
            exception,
            "{ResourceIdentity} {ResourceIdentifier} {NotFoundMessage}",
            ex.ResourceIdentity,
            ex.ResourceIdentifier,
            ex.Message
        );

        var problem = new ProblemDetails
        {
            Title = $"{ex.ResourceIdentity} was not found",
            Status = StatusCodes.Status404NotFound,
            Detail = $"{ex.ResourceIdentity} with {ex.ResourceIdentifier} does not exist",
        };

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, token);

        return true;
    }
}
