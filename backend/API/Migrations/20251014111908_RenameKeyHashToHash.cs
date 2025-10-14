using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RenameKeyHashToHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyKeyHash",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PhysicalKeyKeyHash",
                table: "Users",
                newName: "PhysicalKeyHash");

            migrationBuilder.RenameIndex(
                name: "IX_Users_PhysicalKeyKeyHash",
                table: "Users",
                newName: "IX_Users_PhysicalKeyHash");

            migrationBuilder.RenameColumn(
                name: "KeyHash",
                table: "PhysicalPass",
                newName: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyHash",
                table: "Users",
                column: "PhysicalKeyHash",
                principalTable: "PhysicalPass",
                principalColumn: "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyHash",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PhysicalKeyHash",
                table: "Users",
                newName: "PhysicalKeyKeyHash");

            migrationBuilder.RenameIndex(
                name: "IX_Users_PhysicalKeyHash",
                table: "Users",
                newName: "IX_Users_PhysicalKeyKeyHash");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "PhysicalPass",
                newName: "KeyHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_PhysicalPass_PhysicalKeyKeyHash",
                table: "Users",
                column: "PhysicalKeyKeyHash",
                principalTable: "PhysicalPass",
                principalColumn: "KeyHash");
        }
    }
}
