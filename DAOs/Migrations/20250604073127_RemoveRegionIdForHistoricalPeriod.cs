using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRegionIdForHistoricalPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_historical_periods_regions_region_id",
                table: "historical_periods");

            migrationBuilder.DropIndex(
                name: "IX_historical_periods_region_id",
                table: "historical_periods");

            migrationBuilder.DropColumn(
                name: "region_id",
                table: "historical_periods");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "region_id",
                table: "historical_periods",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_historical_periods_region_id",
                table: "historical_periods",
                column: "region_id");

            migrationBuilder.AddForeignKey(
                name: "FK_historical_periods_regions_region_id",
                table: "historical_periods",
                column: "region_id",
                principalTable: "regions",
                principalColumn: "region_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
