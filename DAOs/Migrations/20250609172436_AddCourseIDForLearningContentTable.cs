using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIDForLearningContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "course_id",
                table: "learning_contents",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_learning_contents_course_id",
                table: "learning_contents",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_learning_contents_courses_course_id",
                table: "learning_contents",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "course_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_learning_contents_courses_course_id",
                table: "learning_contents");

            migrationBuilder.DropIndex(
                name: "IX_learning_contents_course_id",
                table: "learning_contents");

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "learning_contents");
        }
    }
}
