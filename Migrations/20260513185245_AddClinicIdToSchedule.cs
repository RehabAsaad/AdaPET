using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaPET.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicIdToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Schedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ClinicId",
                table: "Schedules",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Clinics_ClinicId",
                table: "Schedules",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Clinics_ClinicId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ClinicId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Schedules");
        }
    }
}
