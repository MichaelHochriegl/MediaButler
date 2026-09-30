using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseStorageLocationsForPhysicalLibrarySources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pre-alpha library data is disposable; deleting libraries cascades to their sources.
            migrationBuilder.Sql("""
                DELETE FROM "Library" WHERE "kind" = 'physical';
                """);

            migrationBuilder.DropIndex(
                name: "IX_LibrarySource_Path",
                table: "LibrarySource");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "LibrarySource",
                newName: "RelativePath");

            migrationBuilder.AddColumn<string>(
                name: "StorageLocationId",
                table: "LibrarySource",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySource_StorageLocationId_RelativePath",
                table: "LibrarySource",
                columns: new[] { "StorageLocationId", "RelativePath" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Logical references cannot restore absolute paths; discard pre-alpha data on rollback too.
            migrationBuilder.Sql("""
                DELETE FROM "Library" WHERE "kind" = 'physical';
                """);

            migrationBuilder.DropIndex(
                name: "IX_LibrarySource_StorageLocationId_RelativePath",
                table: "LibrarySource");

            migrationBuilder.DropColumn(
                name: "StorageLocationId",
                table: "LibrarySource");

            migrationBuilder.RenameColumn(
                name: "RelativePath",
                table: "LibrarySource",
                newName: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_LibrarySource_Path",
                table: "LibrarySource",
                column: "Path",
                unique: true);
        }
    }
}
