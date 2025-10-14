using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RenamePasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhysicalPass_Logs_LastUsedId",
                table: "PhysicalPass");

            migrationBuilder.DropForeignKey(
                name: "FK_PhysicalPass_Users_UserNeptunId",
                table: "PhysicalPass");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhysicalPass",
                table: "PhysicalPass");

            migrationBuilder.RenameTable(
                name: "PhysicalPass",
                newName: "Passes");

            migrationBuilder.RenameIndex(
                name: "IX_PhysicalPass_UserNeptunId",
                table: "Passes",
                newName: "IX_Passes_UserNeptunId");

            migrationBuilder.RenameIndex(
                name: "IX_PhysicalPass_LastUsedId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                newName: "PhysicalPass");

            migrationBuilder.RenameIndex(
                name: "IX_Passes_UserNeptunId",
                table: "PhysicalPass",
                newName: "IX_PhysicalPass_UserNeptunId");

            migrationBuilder.RenameIndex(
                name: "IX_Passes_LastUsedId",
                table: "PhysicalPass",
                newName: "IX_PhysicalPass_LastUsedId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhysicalPass",
                table: "PhysicalPass",
                column: "Hash");

            migrationBuilder.AddForeignKey(
                name: "FK_PhysicalPass_Logs_LastUsedId",
                table: "PhysicalPass",
                column: "LastUsedId",
                principalTable: "Logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhysicalPass_Users_UserNeptunId",
                table: "PhysicalPass",
                column: "UserNeptunId",
                principalTable: "Users",
                principalColumn: "NeptunId");
        }
    }
}
