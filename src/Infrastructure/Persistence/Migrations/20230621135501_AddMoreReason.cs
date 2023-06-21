using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "ImportRequests",
                newName: "StaffReason");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Borrows",
                newName: "StaffReason");

            migrationBuilder.AddColumn<string>(
                name: "ImportReason",
                table: "ImportRequests",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BorrowReason",
                table: "Borrows",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportReason",
                table: "ImportRequests");

            migrationBuilder.DropColumn(
                name: "BorrowReason",
                table: "Borrows");

            migrationBuilder.RenameColumn(
                name: "StaffReason",
                table: "ImportRequests",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "StaffReason",
                table: "Borrows",
                newName: "Reason");
        }
    }
}
