using Libraries.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Common;

namespace Persistence;

/// <summary>
/// Represents the application's database context, inheriting from <see cref="DbContext"/>.
/// </summary>
public class AppDbContext : DbContext
{
    public LibrariesDb Libs { get; private set; }
    
    /// <summary>
    /// Represents the application's database context, inheriting from <see cref="DbContext"/>.
    /// </summary>
    /// <param name="options">
    /// An instance of <see cref="DbContextOptions{TContext}"/> used to configure the context.
    /// </param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Libs = new LibrariesDb(this);
    }
    
    /// <summary>
    /// Configures the model for the database context by applying entity configurations
    /// from dynamically discovered backend assemblies.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to configure the database schema.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var persistenceAssembly in ModuleDescriptors.PersistenceAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(persistenceAssembly);
        }
    }
}