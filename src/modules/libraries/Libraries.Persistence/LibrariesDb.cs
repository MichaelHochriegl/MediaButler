using Domain.Libraries;
using Domain.Libraries.Physical;
using Microsoft.EntityFrameworkCore;

namespace Libraries.Persistence;

public class LibrariesDb(DbContext dbContext)
{
    public DbSet<Library> Libraries => dbContext.Set<Library>();
    public DbSet<LibrarySource> LibrarySources => dbContext.Set<LibrarySource>();
    public DbSet<PhysicalLibrary> PhysicalLibraries => dbContext.Set<PhysicalLibrary>();
    
}