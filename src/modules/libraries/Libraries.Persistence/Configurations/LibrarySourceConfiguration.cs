using Domain.Libraries;
using Domain.Libraries.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libraries.Persistence.Configurations;

public class LibrarySourceConfiguration : IEntityTypeConfiguration<LibrarySource>
{
    public void Configure(EntityTypeBuilder<LibrarySource> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property<LibraryId>("library_id")
            .HasConversion(
                id => id.Value,
                value => LibraryId.From(value))
            .IsRequired();

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => LibrarySourceId.From(value))
            .ValueGeneratedNever();

        builder.Property(x => x.MediaKind)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex("library_id", nameof(LibrarySource.MediaKind));

        builder.HasDiscriminator<string>("kind")
            .HasValue<PhysicalLibrarySource>("physical");
    }
}