using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using EC.TaskBoard.Web.Common.Framework.Cache.Common;

namespace EC.TaskBoard.Web.Common.Framework.Cache.Redis;

public sealed class RedisCacheClient : ICacheClient
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisCacheClient(
        IDistributedCache cache,
        IConnectionMultiplexer multiplexer
    )
    {
        _cache = cache;
        _multiplexer = multiplexer;
    }

    public async Task CreateAsync<TValue>(
        string key,
        TValue value,
        TimeSpan ttl,
        CancellationToken token = default
    ) where TValue : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);

        await _cache.SetAsync(
            key,
            bytes,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            token
        );
    }

    public async Task<bool> CreateIfDoesNotExistAsync(
        string key,
        TimeSpan ttl,
        CancellationToken token = default
    )
    {
        var db = _multiplexer.GetDatabase();

        return await db.StringSetAsync(
            key,
            RedisValue.EmptyString,
            ttl,
            When.NotExists
        );
    }

    public async Task<TValue?> ReadAsync<TValue>(
        string key,
        CancellationToken token = default
    ) where TValue : class
    {
        var bytes = await _cache.GetAsync(key, token);

        if (bytes is null)
            return null;

        return JsonSerializer.Deserialize<TValue>(bytes);
    }

    public async Task DeleteAsync(string key, CancellationToken token = default)
    {
        await _cache.RemoveAsync(key, token);
    }
}
