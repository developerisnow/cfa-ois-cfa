using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OIS.Issuance.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "issuances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    nominal = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    issue_date = table.Column<DateOnly>(type: "date", nullable: false),
                    maturity_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    schedule_json = table.Column<string>(type: "jsonb", nullable: true),
                    dlt_tx_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issuances", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_issuances_asset_id",
                table: "issuances",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_issuances_issuer_id",
                table: "issuances",
                column: "issuer_id");

            migrationBuilder.CreateIndex(
                name: "ix_issuances_status",
                table: "issuances",
                column: "status");

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
                name: "issuances");

            migrationBuilder.DropTable(
                name: "outbox_messages");
        }
    }
}

