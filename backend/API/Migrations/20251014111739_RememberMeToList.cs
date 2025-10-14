using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RememberMeToList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RememberMe_RememberMeRememberHash",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RememberMeRememberHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RememberMeRememberHash",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "UserNeptunId",
                table: "RememberMe",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RememberMe_UserNeptunId",
                table: "RememberMe",
                column: "UserNeptunId");

            migrationBuilder.AddForeignKey(
                name: "FK_RememberMe_Users_UserNeptunId",
                table: "RememberMe",
                column: "UserNeptunId",
                principalTable: "Users",
                principalColumn: "NeptunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RememberMe_Users_UserNeptunId",
                table: "RememberMe");

            migrationBuilder.DropIndex(
                name: "IX_RememberMe_UserNeptunId",
                table: "RememberMe");

            migrationBuilder.DropColumn(
                name: "UserNeptunId",
                table: "RememberMe");

            migrationBuilder.AddColumn<string>(
                name: "RememberMeRememberHash",
                table: "Users",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RememberMeRememberHash",
                table: "Users",
                column: "RememberMeRememberHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RememberMe_RememberMeRememberHash",
                table: "Users",
                column: "RememberMeRememberHash",
                principalTable: "RememberMe",
                principalColumn: "RememberHash");
        }
    }
}
