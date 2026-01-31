using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddFusionCache()
    .WithOptions(opt =>
    {
        // opt.CacheName = "MyCache"; set either here or in AddFusionCache call. To differentiate different services using redis idk
        opt.DefaultEntryOptions.SetDuration(TimeSpan.FromSeconds(10));
        opt.DefaultEntryOptions.SetDistributedCacheDuration(TimeSpan.FromSeconds(30));
        opt.DefaultEntryOptions.SkipBackplaneNotifications = false;
        opt.BackplaneChannelPrefix = "orders-cache"; // "orders-cache.Backplane:v2"
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(_ =>
    {
        // use sp...
        return new RedisCache(new RedisCacheOptions
        {
            Configuration = "localhost"
        });
    })
    .WithBackplane(_ =>
    {
        // invalidates only L1 cache on Remove / Set calls
        var backPlaneOptions = new RedisBackplaneOptions
        {
            Configuration = "localhost"
        };
        var backplane = new RedisBackplane(backPlaneOptions);
        return backplane;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var orders = new List<Order>();

app.MapPost("/orders/{id:int}", async (
    [FromRoute] int id,
    [FromServices] IFusionCache cache,
    CancellationToken ct) =>
{
    // imagine this is event 
    var order = new Order { Id = id, Name = "Name" };
    orders.Add(order);

    // publishes backplane event
    // if there's L1 cache present - updates it from L2
    // since it's cache creation - there's none
    var key = $"order:{id}";
    await cache.SetAsync(key: key, value: order, token: ct);
    
    // hgetall v2:order:1
    // 1) "sldexp"
    // 2) "-1"
    // 3) "absexp"
    // 4) "639054732898706220"
    // 5) "data"
    // 6) "{\"Value\":{\"Id\":1,\"Name\":\"Name\"},\"Timestamp\":639054732598704450,\"LogicalExpirationTimestamp\":639054732898704450,\"Tags\":null,\"Metadata\":null}"

    return Results.Ok();
});

app.MapPut("/orders/{id:int}", async (
    [FromRoute] int id,
    [FromServices] IFusionCache cache,
    CancellationToken ct) =>
{
    // imagine this is event
    // immediately invalidate in case of Set crash to avoid long stale L2
    var key = $"order:{id}";
    await cache.RemoveAsync(key, token: ct);
    
    var order = orders.FirstOrDefault(x => x.Id == id);
    order!.Do();

    // update cache after, in case of failure cache expires after ttl or removed (1st call)
    // there are shit ton of strategies based on non func requirements
    // For lost update scenarios (read + update race condition) use locks or versioning keys
    
    await cache.SetAsync(key: key, value: order, token: ct);
});

app.MapGet("/orders/{id:int}", async (
    [FromRoute] int id,
    [FromServices] IFusionCache cache,
    CancellationToken ct) =>
{
    var key = $"order:{id}";

    // if order is set after calling factory - backplane event published
    var order = await cache.GetOrSetAsync(key: key,
        _ => { return Task.FromResult(orders.FirstOrDefault(x => x.Id == id)); }, token: ct);

    return Results.Ok(order);
});

await app.RunAsync();

public sealed record Order
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public void Do() => Name = "Updated";
}