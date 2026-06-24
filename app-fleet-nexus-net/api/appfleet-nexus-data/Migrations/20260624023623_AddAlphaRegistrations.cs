using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppFleetNexus.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlphaRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alpha_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Roadblocks = table.Column<List<string>>(type: "text[]", nullable: false),
                    CustomRoadblock = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alpha_registrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alpha_registrations_CreatedAt",
                table: "alpha_registrations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_alpha_registrations_Email",
                table: "alpha_registrations",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alpha_registrations");
        }
    }
}
