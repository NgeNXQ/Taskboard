namespace EC.TaskBoard.Web.Shared.Middleware;

internal sealed record CachedResponse(
    int StatusCode,
    string? ContentType,
    string Body
);
