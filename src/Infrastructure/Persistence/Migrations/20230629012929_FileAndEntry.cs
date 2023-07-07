using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FileAndEntry : Migration
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
                name: "CreatedBy",
                table: "Entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Entries",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Entries_CreatedBy",
                table: "Entries",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_OwnerId",
                table: "Entries",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Users_CreatedBy",
                table: "Entries",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Users_OwnerId",
                table: "Entries",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Users_CreatedBy",
                table: "Entries");

            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Users_OwnerId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_CreatedBy",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_OwnerId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "FileExtension",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Entries");
        }
    }
}
