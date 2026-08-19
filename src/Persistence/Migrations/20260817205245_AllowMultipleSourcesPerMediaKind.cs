using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleSourcesPerMediaKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibrarySource_library_id_MediaKind",
                table: "LibrarySource");

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySource_library_id_MediaKind",
                table: "LibrarySource",
                columns: new[] { "library_id", "MediaKind" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibrarySource_library_id_MediaKind",
                table: "LibrarySource");

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySource_library_id_MediaKind",
                table: "LibrarySource",
                columns: new[] { "library_id", "MediaKind" },
                unique: true);
        }
    }
}
