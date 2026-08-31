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

        builder.Property<Guid>("library_id")
            .IsRequired();

        builder.Property(x => x.Id)
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
