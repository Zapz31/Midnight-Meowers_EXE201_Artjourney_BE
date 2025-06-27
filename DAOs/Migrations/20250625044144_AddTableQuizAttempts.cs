using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class AddTableQuizAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleCourseHasEnrolledBasicViewDTOs",
                columns: table => new
                {
                    ModuleId = table.Column<long>(type: "bigint", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CourseId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "ModuleSubModuleCourseIds",
                columns: table => new
                {
                    SubModuleId = table.Column<long>(type: "bigint", nullable: true),
                    ModuleId = table.Column<long>(type: "bigint", nullable: true),
                    CourseId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "QueryResultBFlats",
                columns: table => new
                {
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    CourseTitle = table.Column<string>(type: "text", nullable: true),
                    CourseDescription = table.Column<string>(type: "text", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: true),
                    RegionName = table.Column<string>(type: "text", nullable: true),
                    HistoricalPeriodName = table.Column<string>(type: "text", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_score = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    total_possible_score = table.Column<decimal>(type: "numeric(4,1)", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    time_taken = table.Column<TimeSpan>(type: "interval", nullable: true),
                    learning_content_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_learning_contents_learning_content_id",
                        column: x => x.learning_content_id,
                        principalTable: "learning_contents",
                        principalColumn: "learning_content_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_quiz_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubModuleCourseHasEnrolledBasicViewDTOs",
                columns: table => new
                {
                    SubModuleId = table.Column<long>(type: "bigint", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: true),
                    ModuleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_learning_content_id",
                table: "quiz_attempts",
                column: "learning_content_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_user_id",
                table: "quiz_attempts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleCourseHasEnrolledBasicViewDTOs");

            migrationBuilder.DropTable(
                name: "ModuleSubModuleCourseIds");

            migrationBuilder.DropTable(
                name: "QueryResultBFlats");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "SubModuleCourseHasEnrolledBasicViewDTOs");
        }
    }
}
