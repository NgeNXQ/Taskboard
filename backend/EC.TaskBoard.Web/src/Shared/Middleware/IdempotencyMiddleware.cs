using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Common.Framework.Cache.Common;

namespace EC.TaskBoard.Web.Shared.Middleware;

internal sealed class IdempotencyMiddleware : IMiddleware
{
    private const string IdempotencyHeader = "X-Idempotency-Key";
    private const string ReplayHeader = "X-Idempotency-Replay";

    private readonly ICacheClient _cache;
    private readonly IdempotencyConfig _config;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(
        ICacheClient cache,
        IOptions<IdempotencyConfig> config,
        ILogger<IdempotencyMiddleware> logger
    )
    {
        _cache = cache;
        _logger = logger;
        _config = config.Value;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!TryGetIdempotencyKey(context, out var idempotencyKey))
        {
            await next(context);
            return;
        }

        var keys = BuildCacheKeys(context, idempotencyKey);

        var cached = await TryGetCachedResponseAsync(context, keys.CacheKey);

        if (cached is not null)
        {
            await WriteResponseAsync(context, cached, isReplay: true);
            return;
        }

        if (!await TryAcquireLockAsync(context, keys.LockKey, idempotencyKey))
            return;

        await ExecuteAndCacheResponseAsync(context, next, keys, idempotencyKey);
    }

    private static bool TryGetIdempotencyKey(HttpContext context, out string idempotencyKey)
    {
        idempotencyKey = string.Empty;

        if (!context.Request.Headers.TryGetValue(IdempotencyHeader, out var values))
            return false;

        if (string.IsNullOrWhiteSpace(values))
            return false;

        idempotencyKey = values.ToString();
        return true;
    }

    private static (string CacheKey, string LockKey) BuildCacheKeys(
        HttpContext context,
        string idempotencyKey
    )
    {
        var request = context.Request;

        var cacheKey = $"idempotency:{request.Method}:{request.Path}:{idempotencyKey}";
        var lockKey = $"{cacheKey}:lock";

        return (cacheKey, lockKey);
    }

    private async Task<CachedResponse?> TryGetCachedResponseAsync(
        HttpContext context,
        string cacheKey
    )
    {
        var cached = await _cache.ReadAsync<CachedResponse>(
            cacheKey,
            context.RequestAborted
        );

        if (cached is null)
            return null;

        _logger.LogInformation(
            "Idempotency cache hit for key={CacheKey}", cacheKey
        );

        return cached;
    }

    private async Task<bool> TryAcquireLockAsync(
        HttpContext context,
        string lockKey,
        string idempotencyKey
    )
    {
        var lockAcquired = await _cache.CreateIfDoesNotExistAsync(
            lockKey,
            _config.LockTtl,
            context.RequestAborted
        );

        if (lockAcquired)
            return true;

        _logger.LogWarning(
            "Idempotency lock contention for key={IdempotencyKey}",
            idempotencyKey
        );

        context.Response.Headers["Retry-After"] = "2";
        context.Response.StatusCode = StatusCodes.Status409Conflict;

        await context.Response.WriteAsync(
            "A request with this idempotency key is already in progress.",
            context.RequestAborted
        );

        return false;
    }

    private async Task ExecuteAndCacheResponseAsync(
        HttpContext context,
        RequestDelegate next,
        (string CacheKey, string LockKey) keys,
        string idempotencyKey
    )
    {
        var originalBody = context.Response.Body;

        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            var body = await ReadResponseBodyAsync(buffer, context);

            if (IsSuccessStatusCode(context.Response.StatusCode))
            {
                await CacheResponseAsync(context, keys.CacheKey, body);

                _logger.LogInformation(
                    "Idempotency response cached for key={IdempotencyKey}",
                    idempotencyKey);
            }

            buffer.Position = 0;

            await CopyToOriginalStreamAsync(buffer, originalBody, context);
        }
        finally
        {
            context.Response.Body = originalBody;

            await _cache.DeleteAsync(
                keys.LockKey,
                context.RequestAborted
            );
        }
    }

    private static async Task<string> ReadResponseBodyAsync(
        MemoryStream buffer,
        HttpContext context
    )
    {
        buffer.Position = 0;

        using var reader = new StreamReader(buffer, leaveOpen: true);

        return await reader.ReadToEndAsync(context.RequestAborted);
    }

    private async Task CacheResponseAsync(
        HttpContext context,
        string cacheKey,
        string body
    )
    {
        var response = new CachedResponse(
            context.Response.StatusCode,
            context.Response.ContentType,
            body
        );

        await _cache.CreateAsync(
            cacheKey,
            response,
            _config.ResponseTtl,
            context.RequestAborted
        );
    }

    private static async Task CopyToOriginalStreamAsync(
        MemoryStream buffer,
        Stream originalBody,
        HttpContext context
    )
    {
        await buffer.CopyToAsync(originalBody, context.RequestAborted);
    }

    private static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        CachedResponse cached,
        bool isReplay
    )
    {
        context.Response.StatusCode = cached.StatusCode;
        context.Response.ContentType = cached.ContentType;

        if (isReplay)
            context.Response.Headers[ReplayHeader] = "true";

        await context.Response.WriteAsync(cached.Body);
    }
}
