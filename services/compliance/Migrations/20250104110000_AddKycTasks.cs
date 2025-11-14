using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OIS.Compliance.Migrations
{
    /// <inheritdoc />
    public partial class AddKycTasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kyc_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    investor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_kyc_tasks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_kyc_tasks_investor_id",
                table: "kyc_tasks",
                column: "investor_id");

            migrationBuilder.CreateIndex(
                name: "ix_kyc_tasks_status",
                table: "kyc_tasks",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kyc_tasks");
        }
    }
}

