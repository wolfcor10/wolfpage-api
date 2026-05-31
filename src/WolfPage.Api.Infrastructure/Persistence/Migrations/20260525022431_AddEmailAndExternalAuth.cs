using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndExternalAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "user",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "user",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmedAt",
                table: "user",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [user]
                SET [EmailConfirmed] = 1,
                    [EmailConfirmedAt] = COALESCE([EmailConfirmedAt], SYSUTCDATETIME())
                WHERE [EmailConfirmed] = 0
                """);

            migrationBuilder.CreateTable(
                name: "user_external_login",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_external_login", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_external_login_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_external_login_Provider_Email",
                table: "user_external_login",
                columns: new[] { "Provider", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_user_external_login_Provider_ProviderUserId",
                table: "user_external_login",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_external_login_UserId",
                table: "user_external_login",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_token_Purpose_TokenHash",
                table: "user_token",
                columns: new[] { "Purpose", "TokenHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_token_UserId",
                table: "user_token",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_external_login");

            migrationBuilder.DropTable(
                name: "user_token");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "user");

            migrationBuilder.DropColumn(
                name: "EmailConfirmedAt",
                table: "user");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "user",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
