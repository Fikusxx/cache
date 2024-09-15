using CacheAside.Infra;
using Microsoft.Extensions.Caching.Distributed;

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

        builder.Services.AddScoped<OrderRepository>();
        builder.Services.AddScoped<IOrderRepository>(sp =>
        {
            var cache = sp.GetRequiredService<IDistributedCache>();
            var repo = sp.GetRequiredService<OrderRepository>();
            return new OrderRepositoryDecorator(repo, cache);
        });
    }
}
