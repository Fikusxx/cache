using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheAside.Infra;

public static class CacheExtensions
{
    private static readonly DistributedCacheEntryOptions Options = new()
        { SlidingExpiration = TimeSpan.FromSeconds(10) };

    public static async Task<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key,
        Func<CancellationToken, Task<T>> factory,
        DistributedCacheEntryOptions? options = null,
        CancellationToken token = default)
    {
        var cachedValue = await cache.GetStringAsync(key, token);

        T? value;

        if (string.IsNullOrWhiteSpace(cachedValue) == false)
        {
            value = JsonSerializer.Deserialize<T>(cachedValue);
            return value;
        }

        value = await factory(token);

        if (value is null)
            return default;

        await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options ?? Options, token);

        return value;
    }
}