namespace CacheAside.Features;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Orders
    {
        public const string GetById = $"{ApiBase}/orders/{{id:int}}";
    }
}