namespace Output.Endpoints;

public static class EndpointExtensions
{
    private const string AuthTag = "auth";
    
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty);
        group.WithTags(AuthTag);
        group.MapAuthEndpoint();

        return app;
    }
}