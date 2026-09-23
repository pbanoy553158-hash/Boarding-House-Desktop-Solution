using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBCRM2.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTenantAndContextSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Tenants",
                newName: "Sex");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Tenants",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Tenants",
                newName: "EmergencyRelationship");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualMoveOutDate",
                table: "Tenants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "MoveInDate",
                table: "Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RoomType",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BillingId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    MonthlyRentalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_BranchId",
                table: "Tenants",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BillingId",
                table: "Payments",
                column: "BillingId");

            migrationBuilder.CreateIndex(
                name: "IX_Billings_TenantId",
                table: "Billings",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Billings_BillingId",
                table: "Payments",
                column: "BillingId",
                principalTable: "Billings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Branches_BranchId",
                table: "Tenants",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Billings_BillingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Branches_BranchId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "Billings");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_BranchId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BillingId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ActualMoveOutDate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MoveInDate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "BillingId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Sex",
                table: "Tenants",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Tenants",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "EmergencyRelationship",
                table: "Tenants",
                newName: "FirstName");
        }
    }
}
