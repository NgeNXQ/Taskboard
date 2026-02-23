using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using EC.TaskBoard.Web.Common.Foundation.Domain;

namespace EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Handlers;

internal sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;

    public DomainExceptionHandler(ILogger<DomainExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken token
    )
    {
        if (exception is not DomainException ex)
            return false;

        if (context.Response.HasStarted)
            return false;

        _logger.LogWarning(exception, ex.Message);

        var problem = new ProblemDetails
        {
            Title = ex.Message,
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = ex.Message,
        };

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, token);

        return true;
    }
}
