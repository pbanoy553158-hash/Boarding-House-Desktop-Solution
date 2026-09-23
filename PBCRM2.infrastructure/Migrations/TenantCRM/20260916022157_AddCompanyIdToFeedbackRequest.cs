using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBCRM2.infrastructure.Migrations.TenantCRM
{
    /// <inheritdoc />
    public partial class AddCompanyIdToFeedbackRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "FeedbackRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "FeedbackRequests");
        }
    }
}
