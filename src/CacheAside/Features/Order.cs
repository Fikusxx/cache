namespace CacheAside.Features;

public sealed record Order
{
    public required int Id { get; init; }
    public required string Number { get; init; }
}