using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFusionCache()
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(sp =>
    {
        // use sp...
        return new RedisCache(new RedisCacheOptions
        {
            Configuration = "localhost"
        });
    })
    .WithDefaultEntryOptions(opt =>
    {
        opt.SetDistributedCacheDuration(TimeSpan.FromSeconds(30));
        opt.SetDuration(TimeSpan.FromSeconds(10));
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/fusion", async ([FromServices] IFusionCache cache, CancellationToken ct) =>
{
    var opt = new FusionCacheEntryOptions
    {
        SkipDistributedCacheRead = true
    };
    var order = new Order(1);
    var memory = cache.GetOrDefault<Order>("order:1", options: opt, token: ct);
    var cached = await cache.GetOrSetAsync(key: "order:1", order, token: ct);

    return Results.Ok(new { inMemory = memory, cached });
});

await app.RunAsync();

public sealed record Order(int Id);