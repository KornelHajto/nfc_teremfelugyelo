using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class FixLastUsedNotToBeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys");

            migrationBuilder.AlterColumn<int>(
                name: "LastUsedId",
                table: "Keys",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys",
                column: "LastUsedId",
                principalTable: "Logs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys");

            migrationBuilder.AlterColumn<int>(
                name: "LastUsedId",
                table: "Keys",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys",
                column: "LastUsedId",
                principalTable: "Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
