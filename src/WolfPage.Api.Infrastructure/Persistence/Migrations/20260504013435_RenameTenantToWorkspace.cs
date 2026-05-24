using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WolfPage.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTenantToWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_page_tenant_TenantId",
                table: "page");

            migrationBuilder.DropForeignKey(
                name: "FK_page_generation_request_tenant_TenantId",
                table: "page_generation_request");

            migrationBuilder.DropForeignKey(
                name: "FK_user_tenant_TenantId",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_TenantId_Email",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_page_Slug",
                table: "page");

            migrationBuilder.DropIndex(
                name: "IX_page_TenantId",
                table: "page");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenant",
                table: "tenant");

            migrationBuilder.RenameTable(
                name: "tenant",
                newName: "workspace");

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceType",
                table: "workspace",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Business");

            migrationBuilder.AddColumn<string>(
                name: "ProfileType",
                table: "workspace",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Business");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workspace",
                table: "workspace",
                column: "Id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "page_generation_request",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_page_generation_request_TenantId",
                table: "page_generation_request",
                newName: "IX_page_generation_request_WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "page",
                newName: "WorkspaceId");

            migrationBuilder.CreateTable(
                name: "workspace_member",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workspace_member_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workspace_member_user_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workspace_member_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workspace_member_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [workspace_member]
                    ([Id], [UserId], [RoleId], [WorkspaceId], [InvitedByUserId], [Status], [CreatedAt], [UpdatedAt], [JoinedAt], [InvitedAt], [RemovedAt])
                SELECT
                    NEWID(),
                    ur.[UserId],
                    ur.[RoleId],
                    u.[TenantId],
                    NULL,
                    N'Active',
                    ur.[AssignedAt],
                    SYSUTCDATETIME(),
                    ur.[AssignedAt],
                    NULL,
                    NULL
                FROM [user_role] ur
                INNER JOIN [user] u ON u.[Id] = ur.[UserId];
                """);

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_WorkspaceId_Slug",
                table: "page",
                columns: new[] { "WorkspaceId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workspace_member_InvitedByUserId",
                table: "workspace_member",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_member_RoleId",
                table: "workspace_member",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_member_UserId",
                table: "workspace_member",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_member_WorkspaceId_UserId_RoleId",
                table: "workspace_member",
                columns: new[] { "WorkspaceId", "UserId", "RoleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_page_workspace_WorkspaceId",
                table: "page",
                column: "WorkspaceId",
                principalTable: "workspace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_page_generation_request_workspace_WorkspaceId",
                table: "page_generation_request",
                column: "WorkspaceId",
                principalTable: "workspace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_page_workspace_WorkspaceId",
                table: "page");

            migrationBuilder.DropForeignKey(
                name: "FK_page_generation_request_workspace_WorkspaceId",
                table: "page_generation_request");

            migrationBuilder.DropIndex(
                name: "IX_user_Email",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_page_WorkspaceId_Slug",
                table: "page");

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_role_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [user_role] ([UserId], [RoleId], [AssignedAt])
                SELECT
                    wm.[UserId],
                    wm.[RoleId],
                    MIN(wm.[CreatedAt])
                FROM [workspace_member] wm
                GROUP BY wm.[UserId], wm.[RoleId];
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "user",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE u
                SET [TenantId] = picked.[WorkspaceId]
                FROM [user] u
                OUTER APPLY (
                    SELECT TOP (1) wm.[WorkspaceId]
                    FROM [workspace_member] wm
                    WHERE wm.[UserId] = u.[Id]
                    ORDER BY CASE WHEN wm.[Status] = N'Active' THEN 0 ELSE 1 END, wm.[CreatedAt]
                ) picked;

                UPDATE [user]
                SET [TenantId] = (SELECT TOP (1) [Id] FROM [workspace])
                WHERE [TenantId] IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "TenantId",
                table: "user",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "workspace_member");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "page_generation_request",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_page_generation_request_WorkspaceId",
                table: "page_generation_request",
                newName: "IX_page_generation_request_TenantId");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "page",
                newName: "TenantId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workspace",
                table: "workspace");

            migrationBuilder.DropColumn(
                name: "WorkspaceType",
                table: "workspace");

            migrationBuilder.DropColumn(
                name: "ProfileType",
                table: "workspace");

            migrationBuilder.RenameTable(
                name: "workspace",
                newName: "tenant");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenant",
                table: "tenant",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_TenantId_Email",
                table: "user",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_Slug",
                table: "page",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_page_TenantId",
                table: "page",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_RoleId",
                table: "user_role",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_page_tenant_TenantId",
                table: "page",
                column: "TenantId",
                principalTable: "tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_page_generation_request_tenant_TenantId",
                table: "page_generation_request",
                column: "TenantId",
                principalTable: "tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_tenant_TenantId",
                table: "user",
                column: "TenantId",
                principalTable: "tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
