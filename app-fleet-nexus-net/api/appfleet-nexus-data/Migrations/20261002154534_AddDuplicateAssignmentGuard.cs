using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFleetNexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateAssignmentGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_vehicle_contacts_VehicleId_ContactId",
                table: "vehicle_contacts",
                columns: new[] { "VehicleId", "ContactId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_contacts_VehicleId_ContactId",
                table: "vehicle_contacts");
        }
    }
}
