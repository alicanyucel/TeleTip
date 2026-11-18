using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeleTip.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FilterUniqueIndexesForSoftDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IoTDevices_SerialNumber",
                table: "IoTDevices");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors");

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_SerialNumber",
                table: "IoTDevices",
                column: "SerialNumber",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IoTDevices_SerialNumber",
                table: "IoTDevices");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors");

            migrationBuilder.CreateIndex(
                name: "IX_IoTDevices_SerialNumber",
                table: "IoTDevices",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Doctors",
                column: "LicenseNumber",
                unique: true);
        }
    }
}
