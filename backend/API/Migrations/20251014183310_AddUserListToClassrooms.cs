using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserListToClassrooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassroomRoomId",
                table: "Users",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClassroomRoomId",
                table: "Users",
                column: "ClassroomRoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Classrooms_ClassroomRoomId",
                table: "Users",
                column: "ClassroomRoomId",
                principalTable: "Classrooms",
                principalColumn: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Classrooms_ClassroomRoomId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClassroomRoomId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClassroomRoomId",
                table: "Users");
        }
    }
}
