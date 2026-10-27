using Domain.Libraries.Physical;
using Domain.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libraries.Persistence.Configurations;

public class PhysicalLibrarySourceConfiguration : IEntityTypeConfiguration<PhysicalLibrarySource>
{
    private const int PhysicalPathMaxLength = 1024;

    public void Configure(EntityTypeBuilder<PhysicalLibrarySource> builder)
    {
        builder.Property(x => x.StorageLocationId)
            .HasConversion(
                location => location.Value,
                value => new StorageLocationId(value))
            .IsRequired();
        
        builder.Property(x => x.RelativePath)
            .HasConversion(
                path => path.Value,
                value => new StorageRelativePath(value))
            .HasMaxLength(PhysicalPathMaxLength)
            .IsRequired();

        builder.HasIndex(x => new { x.StorageLocationId, x.RelativePath })
            .IsUnique();
    }
}