using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSubModuleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "user_sub_module_infos",
                columns: table => new
                {
                    info_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_in = table.Column<TimeSpan>(type: "interval", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    sub_module_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sub_module_infos", x => x.info_id);
                    table.ForeignKey(
                        name: "FK_user_sub_module_infos_sub_modules_sub_module_id",
                        column: x => x.sub_module_id,
                        principalTable: "sub_modules",
                        principalColumn: "sub_module_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_sub_module_infos_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_sub_module_infos_sub_module_id",
                table: "user_sub_module_infos",
                column: "sub_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_sub_module_infos_user_id",
                table: "user_sub_module_infos",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_sub_module_infos");
        }
    }
}
