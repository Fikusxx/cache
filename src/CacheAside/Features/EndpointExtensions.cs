using CacheAside.Features.GetById;

namespace CacheAside.Features;

public static class EndpointExtensions
{
    private const string OrdersTag = "orders";
    
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty);
        group.WithTags(OrdersTag);
        group.MapGetByIdEndpoint();

        return app;
    }
}