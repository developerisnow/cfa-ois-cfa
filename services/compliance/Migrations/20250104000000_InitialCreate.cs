using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OIS.Compliance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "investors_compliance",
                columns: table => new
                {
                    investor_id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    kyc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qualification_tier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    qual_limit = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    qual_used = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_investors_compliance", x => x.investor_id);
                });

            migrationBuilder.CreateTable(
                name: "complaints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    investor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sla_due = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    idem_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_complaints", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_complaints_idem_key",
                table: "complaints",
                column: "idem_key",
                unique: true,
                filter: "\"idem_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_complaints_investor_id",
                table: "complaints",
                column: "investor_id");

            migrationBuilder.CreateIndex(
                name: "ix_complaints_status",
                table: "complaints",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_complaints_category",
                table: "complaints",
                column: "category");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_at_created_at",
                table: "outbox_messages",
                columns: new[] { "processed_at", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "complaints");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "investors_compliance");
        }
    }
}

