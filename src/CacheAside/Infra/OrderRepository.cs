using System.Text.Json;
using CacheAside.Features;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheAside.Infra;

public interface IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken token);
}

public sealed class OrderRepository : IOrderRepository
{
    private static readonly List<Order> Orders = [new Order { Id = 1, Number = "#1" }];

    public async Task<Order?> GetByIdAsync(int id, CancellationToken token)
    {
        await Task.Delay(1, token);
        return Orders.FirstOrDefault(x => x.Id == id);
    }
}

public sealed class OrderRepositoryDecorator : IOrderRepository
{
    private readonly IOrderRepository ctx;
    private readonly IDistributedCache cache;
    private static readonly DistributedCacheEntryOptions Options = new() { SlidingExpiration = TimeSpan.FromSeconds(10) };

    public OrderRepositoryDecorator(IOrderRepository ctx, IDistributedCache cache)
    {
        this.ctx = ctx;
        this.cache = cache;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken token)
    {
        var key = $"order-{id}";

        var cachedValue = await cache.GetStringAsync(key, token);
        Order? order;

        if (string.IsNullOrWhiteSpace(cachedValue) == false)
        {
            order = JsonSerializer.Deserialize<Order>(cachedValue);
            return order;
        }

        order = await ctx.GetByIdAsync(id, token);

        if (order is null)
            return default;

        await cache.SetStringAsync(key, JsonSerializer.Serialize(order), Options, token);

        return order;
    }
}