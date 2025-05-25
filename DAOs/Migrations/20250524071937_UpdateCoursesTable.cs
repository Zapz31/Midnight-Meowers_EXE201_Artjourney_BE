using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoursesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "historical_period_id",
                table: "courses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "prerequisite_course_id",
                table: "courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "region_id",
                table: "courses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_courses_historical_period_id",
                table: "courses",
                column: "historical_period_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_region_id",
                table: "courses",
                column: "region_id");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_historical_periods_historical_period_id",
                table: "courses",
                column: "historical_period_id",
                principalTable: "historical_periods",
                principalColumn: "historical_period_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_courses_regions_region_id",
                table: "courses",
                column: "region_id",
                principalTable: "regions",
                principalColumn: "region_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_historical_periods_historical_period_id",
                table: "courses");

            migrationBuilder.DropForeignKey(
                name: "FK_courses_regions_region_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_historical_period_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_region_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "historical_period_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "prerequisite_course_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "region_id",
                table: "courses");
        }
    }
}
