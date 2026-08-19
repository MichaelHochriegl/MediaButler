namespace Domain.Libraries;

public abstract class LibrarySource
{
    public Guid Id { get; }
    public LibraryMediaKind MediaKind { get; }
    public DateTimeOffset CreatedAt { get; }
    
    protected LibrarySource(LibraryMediaKind mediaKind)
    {
        Id = Guid.CreateVersion7();
        MediaKind = mediaKind;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
