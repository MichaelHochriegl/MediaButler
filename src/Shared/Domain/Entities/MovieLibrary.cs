using MediaButler.Shared.Domain.Common;

namespace MediaButler.Shared.Domain.Entities;

public class MovieLibrary : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    
}