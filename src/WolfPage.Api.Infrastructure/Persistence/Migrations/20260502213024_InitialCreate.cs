using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "template",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "template_version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HtmlTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CssTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_version_template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "template",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "page_generation_request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PageName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ContentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_generation_request", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_generation_request_template_version_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalTable: "template_version",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_page_generation_request_tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "page",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoutePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    HtmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CssContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PublishedUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_page_generation_request_RequestId",
                        column: x => x.RequestId,
                        principalTable: "page_generation_request",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_page_template_version_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalTable: "template_version",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_page_tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "domain_binding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DomainName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Subdomain = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SslStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_binding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_domain_binding_page_PageId",
                        column: x => x.PageId,
                        principalTable: "page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "page_asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_page_asset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_page_asset_page_PageId",
                        column: x => x.PageId,
                        principalTable: "page",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_binding_PageId",
                table: "domain_binding",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_page_RequestId",
                table: "page",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_Slug",
                table: "page",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_TemplateVersionId",
                table: "page",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_page_TenantId",
                table: "page",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_page_asset_PageId",
                table: "page_asset",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_CorrelationId",
                table: "page_generation_request",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_TemplateVersionId",
                table: "page_generation_request",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_page_generation_request_TenantId",
                table: "page_generation_request",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_template_Code",
                table: "template",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_version_TemplateId_VersionNumber",
                table: "template_version",
                columns: new[] { "TemplateId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_binding");

            migrationBuilder.DropTable(
                name: "page_asset");

            migrationBuilder.DropTable(
                name: "page");

            migrationBuilder.DropTable(
                name: "page_generation_request");

            migrationBuilder.DropTable(
                name: "template_version");

            migrationBuilder.DropTable(
                name: "tenant");

            migrationBuilder.DropTable(
                name: "template");
        }
    }
}
