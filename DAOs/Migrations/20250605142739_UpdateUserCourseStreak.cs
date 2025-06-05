using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserCourseStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "checkin_time",
                table: "user_course_streaks",
                newName: "last_access_date");

            migrationBuilder.AddColumn<int>(
                name: "current_streak",
                table: "user_course_streaks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "longest_streak",
                table: "user_course_streaks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "total_days_accessed",
                table: "user_course_streaks",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_streak",
                table: "user_course_streaks");

            migrationBuilder.DropColumn(
                name: "longest_streak",
                table: "user_course_streaks");

            migrationBuilder.DropColumn(
                name: "total_days_accessed",
                table: "user_course_streaks");

            migrationBuilder.RenameColumn(
                name: "last_access_date",
                table: "user_course_streaks",
                newName: "checkin_time");
        }
    }
}
