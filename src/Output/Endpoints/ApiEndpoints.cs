namespace Output.Endpoints;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Auth
    {
        public const string First = $"{ApiBase}/auth/first/{{id:int}}";
        public const string Second = $"{ApiBase}/auth/second/{{id:int}}";
        public const string Evict = $"{ApiBase}/auth/evict";
    }
}