using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnusedKeyProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyHash",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PhysicalKeyHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhysicalKeyHash",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhysicalKeyHash",
                table: "Users",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhysicalKeyHash",
                table: "Users",
                column: "PhysicalKeyHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyHash",
                table: "Users",
                column: "PhysicalKeyHash",
                principalTable: "PhysicalPass",
                principalColumn: "Hash");
        }
    }
}
