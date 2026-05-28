namespace Domain.Libraries;

public abstract class Library
{
    public LibraryId Id { get; private init; }
    public LibraryName Name { get; private set; }
    

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    protected Library()
    {
        Name = null!;
    }
    
    protected Library(LibraryName name)
    {
        Id = LibraryId.New();
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

public readonly record struct LibraryId(Guid Value)
{
    public static LibraryId New() => new(Guid.CreateVersion7());
    public static LibraryId From(Guid value)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("LibraryId cannot be empty",
                nameof(value))
            : new LibraryId(value);
    }
    
    public override string ToString() => Value.ToString();
}