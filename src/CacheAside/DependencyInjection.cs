using CacheAside.Infra;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace CacheAside;

public static class DependencyInjection
{
    public static void AddCache(this WebApplicationBuilder builder)
    {
        // builder.Services.AddDistributedMemoryCache(); // adds in memory impl
        
        // adds IDistributedCache inside
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Cache");
            options.InstanceName = "local-";
        });

        // use redis ext asp core & text json
        // adds IRedisDatabase
        // builder.Services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(sp =>
        // {
        //     // use sp..
        //     return new RedisConfiguration[]
        //     {
        //         new() { ConnectionString = "localhost" }
        //     };
        // });

        builder.Services.AddScoped<OrderRepository>();
        builder.Services.AddScoped<IOrderRepository>(sp =>
        {
            var cache = sp.GetRequiredService<IDistributedCache>();
            var repo = sp.GetRequiredService<OrderRepository>();
            return new OrderRepositoryDecorator(repo, cache);
        });
    }
}
