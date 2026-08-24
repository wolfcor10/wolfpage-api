using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileMediaAndCatalogGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageStoragePath",
                table: "workspace_profile",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoStoragePath",
                table: "workspace_profile",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "workspace_catalog_item_image",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_catalog_item_image", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workspace_catalog_item_image_workspace_catalog_item_CatalogItemId",
                        column: x => x.CatalogItemId,
                        principalTable: "workspace_catalog_item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workspace_catalog_item_image_CatalogItemId_IsPrimary",
                table: "workspace_catalog_item_image",
                columns: new[] { "CatalogItemId", "IsPrimary" },
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_catalog_item_image_CatalogItemId_SortOrder",
                table: "workspace_catalog_item_image",
                columns: new[] { "CatalogItemId", "SortOrder" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO [workspace_catalog_item_image]
                    ([Id], [CatalogItemId], [StoragePath], [ContentType], [IsPrimary], [SortOrder], [CreatedAt])
                SELECT
                    NEWID(),
                    [Id],
                    [ImageStoragePath],
                    CASE
                        WHEN LOWER([ImageStoragePath]) LIKE '%.png' THEN 'image/png'
                        WHEN LOWER([ImageStoragePath]) LIKE '%.webp' THEN 'image/webp'
                        ELSE 'image/jpeg'
                    END,
                    1,
                    0,
                    [UpdatedAt]
                FROM [workspace_catalog_item]
                WHERE [ImageStoragePath] IS NOT NULL
                    AND LTRIM(RTRIM([ImageStoragePath])) <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_catalog_item_image");

            migrationBuilder.DropColumn(
                name: "CoverImageStoragePath",
                table: "workspace_profile");

            migrationBuilder.DropColumn(
                name: "LogoStoragePath",
                table: "workspace_profile");
        }
    }
}
