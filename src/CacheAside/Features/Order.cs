namespace CacheAside.Features;

public sealed record Order
{
    public int Id { get; init; }
    public required string Number { get; init; }
}