using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RenamePassesToKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passes_Logs_LastUsedId",
                table: "Passes");

            migrationBuilder.DropForeignKey(
                name: "FK_Passes_Users_UserNeptunId",
                table: "Passes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Passes",
                table: "Passes");

            migrationBuilder.RenameTable(
                name: "Passes",
                newName: "Keys");

            migrationBuilder.RenameIndex(
                name: "IX_Passes_UserNeptunId",
                table: "Keys",
                newName: "IX_Keys_UserNeptunId");

            migrationBuilder.RenameIndex(
                name: "IX_Passes_LastUsedId",
                table: "Keys",
                newName: "IX_Keys_LastUsedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Keys",
                table: "Keys",
                column: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys",
                column: "LastUsedId",
                principalTable: "Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Keys_Users_UserNeptunId",
                table: "Keys",
                column: "UserNeptunId",
                principalTable: "Users",
                principalColumn: "NeptunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keys_Logs_LastUsedId",
                table: "Keys");

            migrationBuilder.DropForeignKey(
                name: "FK_Keys_Users_UserNeptunId",
                table: "Keys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Keys",
                table: "Keys");

            migrationBuilder.RenameTable(
                name: "Keys",
                newName: "Passes");

            migrationBuilder.RenameIndex(
                name: "IX_Keys_UserNeptunId",
                table: "Passes",
                newName: "IX_Passes_UserNeptunId");

            migrationBuilder.RenameIndex(
                name: "IX_Keys_LastUsedId",
                table: "Passes",
                newName: "IX_Passes_LastUsedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Passes",
                table: "Passes",
                column: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_Passes_Logs_LastUsedId",
                table: "Passes",
                column: "LastUsedId",
                principalTable: "Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Passes_Users_UserNeptunId",
                table: "Passes",
                column: "UserNeptunId",
                principalTable: "Users",
                principalColumn: "NeptunId");
        }
    }
}
