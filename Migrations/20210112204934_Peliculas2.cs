using Microsoft.EntityFrameworkCore.Migrations;

namespace back_end.Migrations
{
    public partial class Peliculas2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extra",
                table: "PeliculasGeneros");

            migrationBuilder.DropColumn(
                name: "Extra",
                table: "PeliculasCines");

            migrationBuilder.DropColumn(
                name: "Extra",
                table: "PeliculasActores");

            migrationBuilder.DropColumn(
                name: "Extra",
                table: "Peliculas");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "PeliculasGeneros",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "PeliculasCines",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "PeliculasActores",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extra",
                table: "Peliculas",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
