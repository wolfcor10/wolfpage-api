using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogItemBlobStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageStoragePath",
                table: "workspace_catalog_item",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageStoragePath",
                table: "workspace_catalog_item");
        }
    }
}
