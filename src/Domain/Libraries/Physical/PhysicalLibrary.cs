namespace Domain.Libraries.Physical;

public sealed class PhysicalLibrary : Library
{
    private readonly List<PhysicalLibrarySource> _sources = [];

    public IReadOnlyCollection<PhysicalLibrarySource> Sources => _sources.AsReadOnly();

    private PhysicalLibrary()
    {
    }

    private PhysicalLibrary(LibraryName name, IEnumerable<PhysicalLibrarySource> sources)
        : base(name)
    {
        foreach (var source in sources)
        {
            AddSourceInternal(source);
        }

        if (_sources.Count == 0)
        {
            throw new ArgumentException("A physical library needs at least one source");
        }
    }

    public static PhysicalLibrary Create(
        LibraryName name,
        IEnumerable<PhysicalLibrarySource> sources)
    {

        return new PhysicalLibrary(name, sources);
    }

    public void AddSource(LibraryMediaKind mediaKind, PhysicalPath path)
    {
        AddSourceInternal(new PhysicalLibrarySource(mediaKind, path));
        MarkUpdated();
    }

    public void RemoveSource(PhysicalLibrarySource source)
    {
        if (!_sources.Contains(source))
        {
            throw new InvalidOperationException("The given source is not part of this library");
        }

        if (_sources.Count == 1)
        {
            throw new InvalidOperationException("A physical library must have at least one source");
        }

        _sources.Remove(source);
        MarkUpdated();
    }

    private void AddSourceInternal(PhysicalLibrarySource source)
    {
        if (_sources.Any(x => x.Path == source.Path))
        {
            throw new InvalidOperationException(
                $"The physical library already has the path '{source.Path.Value}' assigned");
        }

        _sources.Add(source);
    }
}