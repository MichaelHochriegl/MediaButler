using Domain.Libraries.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libraries.Persistence.Configurations;

public class PhysicalLibrarySourceConfiguration : IEntityTypeConfiguration<PhysicalLibrarySource>
{
    private const int PhysicalPathMaxLength = 1024;

    public void Configure(EntityTypeBuilder<PhysicalLibrarySource> builder)
    {
        builder.Property(x => x.Path)
            .HasConversion(
                path => path.Value,
                value => new PhysicalPath(value))
            .HasMaxLength(PhysicalPathMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.Path)
            .IsUnique();
    }
}