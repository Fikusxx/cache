using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace Output.CachePolicies;

public sealed class BaseAuthCachingPolicy : IOutputCachePolicy
{
    public static readonly BaseAuthCachingPolicy Instance = new();

    ValueTask IOutputCachePolicy.CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var attemptOutputCaching = AttemptOutputCaching(context);
        context.EnableOutputCaching = true;
        context.AllowCacheLookup = attemptOutputCaching;
        context.AllowCacheStorage = attemptOutputCaching;
        context.AllowLocking = true;
        
        // Expire()
        context.ResponseExpirationTimeSpan = TimeSpan.FromSeconds(10);
        // SetCacheKeyPrefix()
        context.CacheVaryByRules.CacheKeyPrefix = "auth";
        // removes this part "LOCALHOST:5102" from cache key
        context.CacheVaryByRules.VaryByHost = false;

        // Vary by any query by default
        context.CacheVaryByRules.QueryKeys = "*";
        
        // header to vary by
        // context.CacheVaryByRules.HeaderNames = "x-api-key";

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync
        (OutputCacheContext context, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeResponseAsync
        (OutputCacheContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;

        // Verify existence of cookie headers
        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        // Check response code
        if (response.StatusCode == StatusCodes.Status200OK) 
            return ValueTask.CompletedTask;
        
        context.AllowCacheStorage = false;
        return ValueTask.CompletedTask;

    }

    private static bool AttemptOutputCaching(OutputCacheContext context)
    {
        // Check if the current request fulfills the requirements to be cached
        var request = context.HttpContext.Request;

        // Verify the method
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            return false;
        }
        
        return true;

        // Verify existence of authorization headers
        // remove it if used on [Authorize] endpoints, otherwise caching will not work
        return StringValues.IsNullOrEmpty(request.Headers.Authorization) &&
               request.HttpContext.User?.Identity?.IsAuthenticated != true;
    }
}