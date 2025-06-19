using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAOs.Migrations
{
    /// <inheritdoc />
    public partial class DropStartAtColumnFor2Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "started_at",
                table: "user_sub_module_infos");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "user_module_infos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "user_sub_module_infos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "user_module_infos",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
