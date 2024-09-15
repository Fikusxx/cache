using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Output.CachePolicies;

namespace Output.Endpoints;

/// <summary>
/// Realistically there should be 1 policy for each endpoint (cohesion / vertical slice)
/// </summary>
public static class AuthEndpoint
{
    private const string First = "first";
    private const string Second = "second";
    private const string Evict = "evict";

    // to simulate resource validation
    // could as well be sub value from jwt or smth
    private const string ApiKeyHeader = "x-api-key";

    public static IEndpointRouteBuilder MapAuthEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Auth.First, (
                [FromRoute] int id,
                HttpContext context) =>
            {
                context.Request.Headers.TryGetValue(ApiKeyHeader, out var _);
                // do some resource validation here idk
                
                return Results.Text("done");
            })
            // .CacheOutput(policyName: nameof(BaseAuthCachingPolicy)) // use policy as is
            // .CacheOutput(x => x.Expire(TimeSpan.FromSeconds(10)), true) // build your own
            .CacheOutput(x => x // use existing + add/override some props
                    .AddPolicy<BaseAuthCachingPolicy>()
                    .SetVaryByHeader(ApiKeyHeader) // adds specific header to cache key (if it exists)
                    .Expire(TimeSpan.FromSeconds(30)) // ttl is overriden for convenience
                    .Tag(First), // enables eviction by tag
                excludeDefaultPolicy: true) // otherwise default policy will be used as well, which skips caching for ALL authorized requests
            .WithName(First);

        app.MapGet(ApiEndpoints.Auth.Second, (
                [FromRoute] int id) =>
            {
                return Results.Text("done");
            })
            .CacheOutput(x => x
                    .AddPolicy<BaseAuthCachingPolicy>()
                    .Expire(TimeSpan.FromSeconds(30))
                    .Tag(Second),
                true)
            .WithName(Second);

        // tag should be either "first" or "second"
        app.MapDelete(ApiEndpoints.Auth.Evict, async (
                [FromQuery] string tag,
                [FromServices] IOutputCacheStore store,
                CancellationToken token) =>
            {
                await store.EvictByTagAsync(tag, token);
                return Results.Text("done");
            })
            .WithName(Evict);

        return app;
    }
}