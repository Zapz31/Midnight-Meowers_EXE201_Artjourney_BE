using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnsInCourseReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "course_reviews",
                newName: "rating");

            migrationBuilder.RenameColumn(
                name: "Feedback",
                table: "course_reviews",
                newName: "feedback");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "course_reviews",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "course_reviews",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "course_reviews",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "course_reviews",
                newName: "course_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "rating",
                table: "course_reviews",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "feedback",
                table: "course_reviews",
                newName: "Feedback");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "course_reviews",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "course_reviews",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "course_reviews",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "course_reviews",
                newName: "CourseId");
        }
    }
}
