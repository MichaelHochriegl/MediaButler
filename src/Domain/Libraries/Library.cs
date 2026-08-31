namespace Domain.Libraries;

public abstract class Library
{
    public Guid Id { get; private init; }
    public LibraryName Name { get; private set; }
    

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    protected Library()
    {
        Name = null!;
    }
    
    protected Library(LibraryName name)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Rename(LibraryName name)
    {
        if (Name == name)
        {
            return;
        }
        
        Name = name;
        MarkUpdated();
    }
    
    protected void MarkUpdated() => UpdatedAt = DateTimeOffset.UtcNow;
}
