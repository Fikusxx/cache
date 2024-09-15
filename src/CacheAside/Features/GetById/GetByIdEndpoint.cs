using CacheAside.Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CacheAside.Features.GetById;

public static class GetByIdEndpoint
{
    private const string GetById = "GetById";

    public static IEndpointRouteBuilder MapGetByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Orders.GetById, async (
                [FromRoute] int id,
                [FromServices] IOrderRepository ctx,
                [FromServices] IDistributedCache ctx2,
                CancellationToken token) =>
            {
                // decorator
                var order = await ctx.GetByIdAsync(id, token);
                
                // functional approach, too much coupling imo
                // business handler or repository would know too much
                // var order = await ctx2.GetOrCreateAsync($"order-{id}", async token2 => await ctx.GetByIdAsync(id, token2), null, token);

                return order is null ? Results.NotFound() : Results.Ok(order);
            })
            .WithName(GetById);

        return app;
    }
}