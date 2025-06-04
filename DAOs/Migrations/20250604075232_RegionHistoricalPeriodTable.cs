using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class RegionHistoricalPeriodTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "region_historical_period",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    region_id = table.Column<long>(type: "bigint", nullable: false),
                    historical_period_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_region_historical_period", x => x.id);
                    table.ForeignKey(
                        name: "FK_region_historical_period_historical_periods_historical_peri~",
                        column: x => x.historical_period_id,
                        principalTable: "historical_periods",
                        principalColumn: "historical_period_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_region_historical_period_regions_region_id",
                        column: x => x.region_id,
                        principalTable: "regions",
                        principalColumn: "region_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_region_historical_period_historical_period_id",
                table: "region_historical_period",
                column: "historical_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_region_historical_period_region_id",
                table: "region_historical_period",
                column: "region_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "region_historical_period");
        }
    }
}
