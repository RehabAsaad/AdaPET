using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaPET.Migrations
{
    /// <inheritdoc />
    public partial class FixScheduleDoctorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_DoctorId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Schedules",
                newName: "DoctorUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DoctorId",
                table: "Schedules",
                newName: "IX_Schedules_DoctorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_DoctorUserId",
                table: "Schedules",
                column: "DoctorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_DoctorUserId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "DoctorUserId",
                table: "Schedules",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_DoctorUserId",
                table: "Schedules",
                newName: "IX_Schedules_DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_DoctorId",
                table: "Schedules",
                column: "DoctorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
