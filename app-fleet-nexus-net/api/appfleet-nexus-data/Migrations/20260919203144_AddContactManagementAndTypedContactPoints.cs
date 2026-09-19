using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFleetNexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContactManagementAndTypedContactPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrimaryPhone",
                table: "contacts",
                newName: "UniqueId");

            migrationBuilder.RenameColumn(
                name: "PrimaryEmail",
                table: "contacts",
                newName: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "ContactType",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LicenseExpirationDate",
                table: "contacts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseState",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MedicalCertExpirationDate",
                table: "contacts",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "contacts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "contact_addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AddressLine1 = table.Column<string>(type: "text", nullable: true),
                    AddressLine2 = table.Column<string>(type: "text", nullable: true),
                    AddressLine3 = table.Column<string>(type: "text", nullable: true),
                    AddressLine4 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: false, defaultValue: "USA"),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact_emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_emails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact_phones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_phones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssociationRole = table.Column<string>(type: "text", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_contacts_contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_contacts_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contacts_TenantId_UniqueId",
                table: "contacts",
                columns: new[] { "TenantId", "UniqueId" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"UniqueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_contact_addresses_OwnerType_OwnerId",
                table: "contact_addresses",
                columns: new[] { "OwnerType", "OwnerId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_contact_addresses_TenantId",
                table: "contact_addresses",
                column: "TenantId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_contact_emails_OwnerType_OwnerId",
                table: "contact_emails",
                columns: new[] { "OwnerType", "OwnerId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_contact_emails_TenantId",
                table: "contact_emails",
                column: "TenantId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_contact_phones_OwnerType_OwnerId",
                table: "contact_phones",
                columns: new[] { "OwnerType", "OwnerId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_contact_phones_TenantId",
                table: "contact_phones",
                column: "TenantId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_contacts_ContactId",
                table: "vehicle_contacts",
                column: "ContactId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_contacts_TenantId",
                table: "vehicle_contacts",
                column: "TenantId",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_contacts_VehicleId",
                table: "vehicle_contacts",
                column: "VehicleId",
                filter: "\"IsDeleted\" = false");

            // ─── PostgreSQL Row-Level Security (ADR-004) ─────────────────────────────────
            // contact_phones
            migrationBuilder.Sql("ALTER TABLE contact_phones ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("CREATE POLICY contact_phones_tenant_isolation_policy ON contact_phones USING (\"TenantId\" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);");

            // contact_emails
            migrationBuilder.Sql("ALTER TABLE contact_emails ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("CREATE POLICY contact_emails_tenant_isolation_policy ON contact_emails USING (\"TenantId\" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);");

            // contact_addresses
            migrationBuilder.Sql("ALTER TABLE contact_addresses ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("CREATE POLICY contact_addresses_tenant_isolation_policy ON contact_addresses USING (\"TenantId\" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);");

            // vehicle_contacts
            migrationBuilder.Sql("ALTER TABLE vehicle_contacts ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("CREATE POLICY vehicle_contacts_tenant_isolation_policy ON vehicle_contacts USING (\"TenantId\" = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_addresses");

            migrationBuilder.DropTable(
                name: "contact_emails");

            migrationBuilder.DropTable(
                name: "contact_phones");

            migrationBuilder.DropTable(
                name: "vehicle_contacts");

            migrationBuilder.DropIndex(
                name: "IX_contacts_TenantId_UniqueId",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "ContactType",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "LicenseExpirationDate",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "LicenseState",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "MedicalCertExpirationDate",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "contacts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "contacts");

            migrationBuilder.RenameColumn(
                name: "UniqueId",
                table: "contacts",
                newName: "PrimaryPhone");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "contacts",
                newName: "PrimaryEmail");
        }
    }
}
