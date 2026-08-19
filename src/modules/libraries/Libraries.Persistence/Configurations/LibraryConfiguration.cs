using Domain.Libraries;
using Domain.Libraries.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libraries.Persistence.Configurations;

public class LibraryConfiguration : IEntityTypeConfiguration<Library>
{
    public void Configure(EntityTypeBuilder<Library> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => new LibraryName(value))
            .HasMaxLength(LibraryName.MaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);


        builder
            .HasDiscriminator<string>("kind")
            .HasValue<PhysicalLibrary>("physical");

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
