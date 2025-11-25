namespace AuthPocBackend.Models;

public record Entity
{
    public int Id { get; init; }
    public Substance? Substance { get; init; }
    public Num9000? Num9000 { get; init; }
    public required ICollection<Descriptor> Descriptors { get; init; }
}

public record Substance
{
    public int Id { get; init; }
    public required string InchiKey { get; init; }
    public required string Inchi { get; init; }
}

public record Num9000
{
    public int Id { get; init; }
    public int Num { get; init; }
}

public record Descriptor
{
    public int Id { get; init; }
    public required string Desc { get; init; }
}

public record SearchResult
{
    public required ICollection<string> SearchTerms { get; init; }
    public required ICollection<Entity> Entities { get; init; }
}
