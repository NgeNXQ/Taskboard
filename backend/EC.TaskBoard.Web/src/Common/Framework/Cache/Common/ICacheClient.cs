using System;
using System.Threading;
using System.Threading.Tasks;

namespace EC.TaskBoard.Web.Common.Framework.Cache.Common;

public interface ICacheClient
{
    Task CreateAsync<TValue>(
        string key,
        TValue value,
        TimeSpan ttl,
        CancellationToken token = default
    ) where TValue : class;

    Task<bool> CreateIfDoesNotExistAsync(
        string key,
        TimeSpan ttl,
        CancellationToken token = default
    );

    Task<TValue?> ReadAsync<TValue>(
        string key,
        CancellationToken token = default
    ) where TValue : class;

    Task DeleteAsync(
        string key,
        CancellationToken token = default
    );
}
