using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OIS.Registry.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    owner_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    blocked = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_wallets_owner_type_owner_id",
                table: "wallets",
                columns: new[] { "owner_type", "owner_id" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "holdings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    investor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holdings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_holdings_investor_id_issuance_id",
                table: "holdings",
                columns: new[] { "investor_id", "issuance_id" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    investor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    idem_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dlt_tx_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_orders_idem_key",
                table: "orders",
                column: "idem_key",
                unique: true,
                filter: "\"idem_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_orders_investor_id",
                table: "orders",
                column: "investor_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_issuance_id",
                table: "orders",
                column: "issuance_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateTable(
                name: "tx",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    from_wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    dlt_tx_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tx", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tx_issuance_id",
                table: "tx",
                column: "issuance_id");

            migrationBuilder.CreateIndex(
                name: "ix_tx_from_wallet_id",
                table: "tx",
                column: "from_wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_tx_to_wallet_id",
                table: "tx",
                column: "to_wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_tx_status",
                table: "tx",
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
                name: "holdings");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "tx");

            migrationBuilder.DropTable(
                name: "wallets");
        }
    }
}

