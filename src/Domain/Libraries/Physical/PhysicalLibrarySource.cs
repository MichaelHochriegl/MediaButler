namespace Domain.Libraries.Physical;

public sealed class PhysicalLibrarySource : LibrarySource
{
    public PhysicalPath Path { get; private set; }
    
    public PhysicalLibrarySource(LibraryMediaKind mediaKind, PhysicalPath path) : base(mediaKind)
    {
        Path = path;
    }
}