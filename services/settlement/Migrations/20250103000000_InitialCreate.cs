using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OIS.Settlement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payouts_batch",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    run_date = table.Column<DateOnly>(type: "date", nullable: false),
                    issuance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    idem_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payouts_batch", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payouts_batch_idem_key",
                table: "payouts_batch",
                column: "idem_key",
                unique: true,
                filter: "\"idem_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_batch_run_date",
                table: "payouts_batch",
                column: "run_date");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_batch_issuance_id",
                table: "payouts_batch",
                column: "issuance_id");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_batch_status",
                table: "payouts_batch",
                column: "status");

            migrationBuilder.CreateTable(
                name: "payouts_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    investor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    bank_ref = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dlt_tx_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payouts_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_payouts_item_batch",
                        column: x => x.batch_id,
                        principalTable: "payouts_batch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payouts_item_batch_id",
                table: "payouts_item",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_item_investor_id",
                table: "payouts_item",
                column: "investor_id");

            migrationBuilder.CreateIndex(
                name: "ix_payouts_item_status",
                table: "payouts_item",
                column: "status");

            migrationBuilder.CreateTable(
                name: "reconciliation_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_log", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_log_batch_id",
                table: "reconciliation_log",
                column: "batch_id");

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
                name: "payouts_item");

            migrationBuilder.DropTable(
                name: "reconciliation_log");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "payouts_batch");
        }
    }
}

