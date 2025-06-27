using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class FixQuestionOptionIdConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_options_questions_question_option_id",
                table: "question_options");

            migrationBuilder.AlterColumn<long>(
                name: "question_option_id",
                table: "question_options",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_question_options_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.AddForeignKey(
                name: "FK_question_options_questions_question_id",
                table: "question_options",
                column: "question_id",
                principalTable: "questions",
                principalColumn: "question_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_question_options_questions_question_id",
                table: "question_options");

            migrationBuilder.DropIndex(
                name: "IX_question_options_question_id",
                table: "question_options");

            migrationBuilder.AlterColumn<long>(
                name: "question_option_id",
                table: "question_options",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_question_options_questions_question_option_id",
                table: "question_options",
                column: "question_option_id",
                principalTable: "questions",
                principalColumn: "question_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
