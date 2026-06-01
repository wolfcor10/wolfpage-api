using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalTemplatePageGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_page_page_generation_request_RequestId",
                table: "page");

            migrationBuilder.DropIndex(
                name: "IX_page_RequestId",
                table: "page");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateVersionId",
                table: "page_generation_request",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PageId",
                table: "page_generation_request",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SelectedTemplateId",
                table: "page_generation_request",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "general-business");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateVersionId",
                table: "page",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestId",
                table: "page",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "page",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "page",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessCategory",
                table: "page",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessDescription",
                table: "page",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessName",
                table: "page",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "page",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedFilePath",
                table: "page",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroImageUrl",
                table: "page",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroSubtitle",
                table: "page",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroTitle",
                table: "page",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "page",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "page",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "page",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedTemplateId",
                table: "page",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "general-business");

            migrationBuilder.AddColumn<string>(
                name: "SocialLinksJson",
                table: "page",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "page",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "page_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: false),
                    Price = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_item_page_PageId",
                        column: x => x.PageId,
                        principalTable: "page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                UPDATE page
                SET
                    SelectedTemplateId = CASE WHEN SelectedTemplateId = '' THEN 'general-business' ELSE SelectedTemplateId END,
                    BusinessName = CASE WHEN BusinessName = '' THEN Title ELSE BusinessName END,
                    HeroTitle = CASE WHEN HeroTitle = '' THEN Title ELSE HeroTitle END,
                    BusinessDescription = CASE WHEN BusinessDescription = '' THEN Title ELSE BusinessDescription END;

                UPDATE page_generation_request
                SET SelectedTemplateId = CASE WHEN SelectedTemplateId = '' THEN 'general-business' ELSE SelectedTemplateId END;

                UPDATE request
                SET PageId = page.Id
                FROM page_generation_request AS request
                INNER JOIN page ON page.RequestId = request.Id
                WHERE request.PageId = '00000000-0000-0000-0000-000000000000';

                INSERT INTO page (
                    Id,
                    WorkspaceId,
                    TemplateVersionId,
                    RequestId,
                    SelectedTemplateId,
                    Title,
                    Slug,
                    RoutePath,
                    HtmlContent,
                    CssContent,
                    JsContent,
                    BusinessName,
                    BusinessCategory,
                    BusinessDescription,
                    LogoUrl,
                    HeroTitle,
                    HeroSubtitle,
                    HeroImageUrl,
                    Phone,
                    Email,
                    Address,
                    WhatsApp,
                    OpeningHours,
                    SocialLinksJson,
                    GeneratedFilePath,
                    Status,
                    PublishedUrl,
                    CreatedAt,
                    UpdatedAt
                )
                SELECT
                    NEWID(),
                    request.WorkspaceId,
                    request.TemplateVersionId,
                    request.Id,
                    'general-business',
                    request.PageName,
                    LEFT(CONCAT(request.Slug, '-', REPLACE(CONVERT(nvarchar(36), request.Id), '-', '')), 150),
                    CONCAT('/', LEFT(CONCAT(request.Slug, '-', REPLACE(CONVERT(nvarchar(36), request.Id), '-', '')), 150)),
                    NULL,
                    NULL,
                    NULL,
                    request.PageName,
                    NULL,
                    request.PageName,
                    NULL,
                    request.PageName,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    CASE
                        WHEN request.Status = 'Completed' THEN 'Generated'
                        WHEN request.Status = 'Failed' THEN 'Failed'
                        ELSE 'PendingGeneration'
                    END,
                    NULL,
                    request.CreatedAt,
                    COALESCE(request.ProcessedAt, request.CreatedAt)
                FROM page_generation_request AS request
                WHERE request.PageId = '00000000-0000-0000-0000-000000000000';

                UPDATE request
                SET PageId = page.Id
                FROM page_generation_request AS request
                INNER JOIN page ON page.RequestId = request.Id
                WHERE request.PageId = '00000000-0000-0000-0000-000000000000';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request",
                column: "PageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_RequestId",
                table: "page",
                column: "RequestId",
                unique: true,
                filter: "[RequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_page_item_PageId_SortOrder",
                table: "page_item",
                columns: new[] { "PageId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_page_generation_request_page_PageId",
                table: "page_generation_request",
                column: "PageId",
                principalTable: "page",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_page_generation_request_page_PageId",
                table: "page_generation_request");

            migrationBuilder.DropTable(
                name: "page_item");

            migrationBuilder.DropIndex(
                name: "IX_page_generation_request_PageId",
                table: "page_generation_request");

            migrationBuilder.DropIndex(
                name: "IX_page_RequestId",
                table: "page");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "page_generation_request");

            migrationBuilder.DropColumn(
                name: "SelectedTemplateId",
                table: "page_generation_request");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "page");

            migrationBuilder.DropColumn(
                name: "BusinessCategory",
                table: "page");

            migrationBuilder.DropColumn(
                name: "BusinessDescription",
                table: "page");

            migrationBuilder.DropColumn(
                name: "BusinessName",
                table: "page");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "page");

            migrationBuilder.DropColumn(
                name: "GeneratedFilePath",
                table: "page");

            migrationBuilder.DropColumn(
                name: "HeroImageUrl",
                table: "page");

            migrationBuilder.DropColumn(
                name: "HeroSubtitle",
                table: "page");

            migrationBuilder.DropColumn(
                name: "HeroTitle",
                table: "page");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "page");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "page");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "page");

            migrationBuilder.DropColumn(
                name: "SelectedTemplateId",
                table: "page");

            migrationBuilder.DropColumn(
                name: "SocialLinksJson",
                table: "page");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "page");

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateVersionId",
                table: "page_generation_request",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TemplateVersionId",
                table: "page",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestId",
                table: "page",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HtmlContent",
                table: "page",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_RequestId",
                table: "page",
                column: "RequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_page_page_generation_request_RequestId",
                table: "page",
                column: "RequestId",
                principalTable: "page_generation_request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
