using Domain.Libraries.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Libraries.Persistence.Configurations;

public class PhysicalLibraryConfiguration : IEntityTypeConfiguration<PhysicalLibrary>
{
    public void Configure(EntityTypeBuilder<PhysicalLibrary> builder)
    {
        builder.Navigation(x => x.Sources)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Sources)
            .WithOne()
            .HasForeignKey("library_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}