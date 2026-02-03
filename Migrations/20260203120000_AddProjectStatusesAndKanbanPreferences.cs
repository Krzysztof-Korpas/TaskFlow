using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Migrations;

public partial class AddProjectStatusesAndKanbanPreferences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProjectStatuses",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProjectId = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectStatuses", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProjectStatuses_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "KanbanColumnPreferences",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProjectId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: false),
                StatusId = table.Column<int>(type: "integer", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                IsVisible = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_KanbanColumnPreferences", x => x.Id);
                table.ForeignKey(
                    name: "FK_KanbanColumnPreferences_ProjectStatuses_StatusId",
                    column: x => x.StatusId,
                    principalTable: "ProjectStatuses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_KanbanColumnPreferences_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_KanbanColumnPreferences_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddColumn<int>(
            name: "StatusId",
            table: "Tickets",
            type: "integer",
            nullable: true);

        migrationBuilder.Sql("""
            INSERT INTO "ProjectStatuses" ("ProjectId", "Name", "SortOrder", "IsDefault")
            SELECT p."Id", v."Name", v."SortOrder", v."IsDefault"
            FROM "Projects" p
            CROSS JOIN (VALUES
                (0, 'Do zrobienia', TRUE),
                (1, 'W toku', FALSE),
                (2, 'Do przeglądu', FALSE),
                (3, 'Zrobione', FALSE)
            ) AS v("SortOrder", "Name", "IsDefault");
            """);

        migrationBuilder.Sql("""
            UPDATE "Tickets" t
            SET "StatusId" = s."Id"
            FROM "ProjectStatuses" s
            WHERE s."ProjectId" = t."ProjectId"
              AND s."SortOrder" = t."Status";
            """);

        migrationBuilder.AlterColumn<int>(
            name: "StatusId",
            table: "Tickets",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "Status",
            table: "Tickets");

        migrationBuilder.CreateIndex(
            name: "IX_KanbanColumnPreferences_ProjectId_UserId_StatusId",
            table: "KanbanColumnPreferences",
            columns: new[] { "ProjectId", "UserId", "StatusId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_KanbanColumnPreferences_StatusId",
            table: "KanbanColumnPreferences",
            column: "StatusId");

        migrationBuilder.CreateIndex(
            name: "IX_KanbanColumnPreferences_UserId",
            table: "KanbanColumnPreferences",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ProjectStatuses_ProjectId_Name",
            table: "ProjectStatuses",
            columns: new[] { "ProjectId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProjectStatuses_ProjectId",
            table: "ProjectStatuses",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Tickets_StatusId",
            table: "Tickets",
            column: "StatusId");

        migrationBuilder.AddForeignKey(
            name: "FK_Tickets_ProjectStatuses_StatusId",
            table: "Tickets",
            column: "StatusId",
            principalTable: "ProjectStatuses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Tickets_ProjectStatuses_StatusId",
            table: "Tickets");

        migrationBuilder.DropTable(
            name: "KanbanColumnPreferences");

        migrationBuilder.DropTable(
            name: "ProjectStatuses");

        migrationBuilder.DropIndex(
            name: "IX_Tickets_StatusId",
            table: "Tickets");

        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "Tickets",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.DropColumn(
            name: "StatusId",
            table: "Tickets");
    }
}
