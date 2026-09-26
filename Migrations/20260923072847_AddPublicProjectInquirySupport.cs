using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicProjectInquirySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectInquiries_Clients_ClientId",
                table: "ProjectInquiries");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "ProjectInquiries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "ProjectInquiries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "ProjectInquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "ProjectInquiries",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "ProjectInquiries",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceRequired",
                table: "ProjectInquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectInquiries_Clients_ClientId",
                table: "ProjectInquiries",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectInquiries_Clients_ClientId",
                table: "ProjectInquiries");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "ProjectInquiries");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "ProjectInquiries");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "ProjectInquiries");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "ProjectInquiries");

            migrationBuilder.DropColumn(
                name: "ServiceRequired",
                table: "ProjectInquiries");

            migrationBuilder.AlterColumn<int>(
                name: "ClientId",
                table: "ProjectInquiries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectInquiries_Clients_ClientId",
                table: "ProjectInquiries",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
