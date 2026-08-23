using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultiplePageGenerationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request");

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request");

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request",
                column: "PageId",
                unique: true);
        }
    }
}
