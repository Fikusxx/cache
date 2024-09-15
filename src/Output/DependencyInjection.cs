using Output.CachePolicies;

namespace Output;

public static class DependencyInjection
{
    public static void AddCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddOutputCache(options =>
        {
            // makes route as is, instead of all caps
            // "instance-__MSOCV_custom-\x1eGET\x1eHTTP\x1eLOCALHOST:5102/api/auth/1\x1eQ\x1e*="
            // "instance-__MSOCV_custom-\x1eGET\x1eHTTP\x1eLOCALHOST:5102/API/AUTH/1\x1eQ\x1e*="
            options.UseCaseSensitivePaths = true;
            
            // add custom policy to the list of available policies (not basic) used by name .CacheOutput(policyName: nameof(BaseAuthCachingPolicy))
            options.AddPolicy(nameof(BaseAuthCachingPolicy), b => b
                    .AddPolicy<BaseAuthCachingPolicy>()
                    
                    // all these options can be defined inside the class itself
                    .SetVaryByHost(false) // removes this part "LOCALHOST:5102" from cache key
                    .Expire(TimeSpan.FromSeconds(10))
                    .SetCacheKeyPrefix("auth")
                    .With(c => c.HttpContext.Request.Path.StartsWithSegments("/api/auth"))
                    .Tag("custom"), // allows to remove all cached data by this tag(s)
                excludeDefaultPolicy: true); // skips caching for [Authorized] endpoints if not set to "true"
            
            // same thing except for excludeDefaultPolicy not being available in this method
            // options.AddPolicy(nameof(BaseAuthCachingPolicy), BaseAuthCachingPolicy.Instance); 
            
            // add to base policies list
            // options.AddBasePolicy(b => b
            //         .AddPolicy<BaseAuthCachingPolicy>()
            //         .Expire(TimeSpan.FromSeconds(10))
            //         .Tag("basic"),
            //     excludeDefaultPolicy: true
            // );
        });
        
        builder.Services.AddStackExchangeRedisOutputCache(x =>
        {
            x.Configuration = builder.Configuration.GetConnectionString("Cache");
            x.InstanceName = $"instance-"; // to distinguish cache from different contexts (services)
        });
    }
}
