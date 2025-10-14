using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseListToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Picture",
                table: "Users",
                type: "longblob",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "UserNeptunId",
                table: "Courses",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserNeptunId",
                table: "Courses",
                column: "UserNeptunId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_UserNeptunId",
                table: "Courses",
                column: "UserNeptunId",
                principalTable: "Users",
                principalColumn: "NeptunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_UserNeptunId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserNeptunId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserNeptunId",
                table: "Courses");
        }
    }
}
