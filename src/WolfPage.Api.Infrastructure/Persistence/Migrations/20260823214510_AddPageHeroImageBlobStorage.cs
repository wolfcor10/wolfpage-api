using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPageHeroImageBlobStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroImageStoragePath",
                table: "page",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroImageStoragePath",
                table: "page");
        }
    }
}
