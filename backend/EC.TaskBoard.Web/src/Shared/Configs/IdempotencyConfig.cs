using System;

namespace EC.TaskBoard.Web.Shared.Configs;

internal sealed class IdempotencyConfig
{
    internal TimeSpan LockTtl { get; init; } = TimeSpan.FromSeconds(30);
    internal TimeSpan ResponseTtl { get; init; } = TimeSpan.FromHours(24);
}
