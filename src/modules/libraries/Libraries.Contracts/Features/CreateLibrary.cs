namespace Libraries.Contracts.Features;

public record CreateLibraryRequest(string Name)
{
    public string Name { get; init; } = Name;
}

public record CreateLibraryResponse(Guid Id)
{
    public Guid Id { get; init; } = Id;
}