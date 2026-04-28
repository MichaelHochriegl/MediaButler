namespace Domain.Libraries;

public abstract class LibrarySource
{
    public LibrarySourceId Id { get; }
    public LibraryMediaKind MediaKind { get; }
    public DateTimeOffset CreatedAt { get; }
    
    protected LibrarySource(LibraryMediaKind mediaKind)
    {
        Id = LibrarySourceId.New();
        MediaKind = mediaKind;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

public readonly record struct LibrarySourceId(Guid Value)
{
    public static LibrarySourceId New() => new(Guid.CreateVersion7());
    public static LibrarySourceId From(Guid value)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("LibrarySourceId cannot be empty",
                nameof(value))
            : new LibrarySourceId(value);
    }
    
    public override string ToString() => Value.ToString();
}