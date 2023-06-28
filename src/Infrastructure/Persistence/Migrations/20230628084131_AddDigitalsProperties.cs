using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalsProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileExtension",
                table: "Files",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Entries",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "UploaderId",
                table: "Entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Entries_UploaderId",
                table: "Entries",
                column: "UploaderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Users_UploaderId",
                table: "Entries",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Users_UploaderId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_UploaderId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "UploaderId",
                table: "Entries");
        }
    }
}
