using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LockerOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Lockers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Lockers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_OwnerId",
                table: "Lockers",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_Users_OwnerId",
                table: "Lockers",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_Users_OwnerId",
                table: "Lockers");

            migrationBuilder.DropIndex(
                name: "IX_Lockers_OwnerId",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Lockers");
        }
    }
}
