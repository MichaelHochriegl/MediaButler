using Microsoft.EntityFrameworkCore;
using Modules.Common;

namespace Backend.Data;

/// <summary>
/// Represents the application's database context, inheriting from <see cref="DbContext"/>.
/// </summary>
/// <param name="options">
/// An instance of <see cref="DbContextOptions{TContext}"/> used to configure the context.
/// </param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Configures the model for the database context by applying entity configurations
    /// from dynamically discovered backend assemblies.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance used to configure the database schema.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var backendAssembly in ModuleDescriptors.BackendAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(backendAssembly);
        }
    }
}