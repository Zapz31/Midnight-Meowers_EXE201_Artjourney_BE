using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "challenge_items",
                columns: table => new
                {
                    challenge_item_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_type = table.Column<string>(type: "text", nullable: true),
                    item_content = table.Column<string>(type: "text", nullable: true),
                    item_order = table.Column<int>(type: "integer", nullable: true),
                    hint = table.Column<string>(type: "text", nullable: true),
                    additional_data = table.Column<string>(type: "text", nullable: true),
                    learning_content_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challenge_items", x => x.challenge_item_id);
                    table.ForeignKey(
                        name: "FK_challenge_items_learning_contents_learning_content_id",
                        column: x => x.learning_content_id,
                        principalTable: "learning_contents",
                        principalColumn: "learning_content_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_challenge_items_learning_content_id",
                table: "challenge_items",
                column: "learning_content_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "challenge_items");
        }
    }
}
