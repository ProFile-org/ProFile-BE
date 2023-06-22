using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoggingNowHasObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentLogs_Documents_ObjectId",
                table: "DocumentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FolderLogs_Folders_ObjectId",
                table: "FolderLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerLogs_Lockers_ObjectId",
                table: "LockerLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestLogs_Documents_ObjectId",
                table: "RequestLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomLogs_Rooms_ObjectId",
                table: "RoomLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLogs_Users_ObjectId",
                table: "UserLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserLogs_ObjectId",
                table: "UserLogs");

            migrationBuilder.DropIndex(
                name: "IX_RoomLogs_ObjectId",
                table: "RoomLogs");

            migrationBuilder.DropIndex(
                name: "IX_RequestLogs_ObjectId",
                table: "RequestLogs");

            migrationBuilder.DropIndex(
                name: "IX_LockerLogs_ObjectId",
                table: "LockerLogs");

            migrationBuilder.DropIndex(
                name: "IX_FolderLogs_ObjectId",
                table: "FolderLogs");

            migrationBuilder.DropIndex(
                name: "IX_DocumentLogs_ObjectId",
                table: "DocumentLogs");

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "UserLogs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "UserLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_ObjectId",
                table: "UserLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomLogs_ObjectId",
                table: "RoomLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_ObjectId",
                table: "RequestLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLogs_ObjectId",
                table: "LockerLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderLogs_ObjectId",
                table: "FolderLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_ObjectId",
                table: "DocumentLogs",
                column: "ObjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentLogs_Documents_ObjectId",
                table: "DocumentLogs",
                column: "ObjectId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FolderLogs_Folders_ObjectId",
                table: "FolderLogs",
                column: "ObjectId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LockerLogs_Lockers_ObjectId",
                table: "LockerLogs",
                column: "ObjectId",
                principalTable: "Lockers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestLogs_Documents_ObjectId",
                table: "RequestLogs",
                column: "ObjectId",
                principalTable: "Documents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomLogs_Rooms_ObjectId",
                table: "RoomLogs",
                column: "ObjectId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogs_Users_ObjectId",
                table: "UserLogs",
                column: "ObjectId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
