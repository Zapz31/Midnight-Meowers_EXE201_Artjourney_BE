using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToLearningContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "learning_contents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "learning_contents");
        }
    }
}
