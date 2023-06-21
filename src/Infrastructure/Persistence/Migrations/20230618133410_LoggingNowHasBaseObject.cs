using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoggingNowHasBaseObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportRequests_DocumentId",
                table: "ImportRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "RequestLogs");

            migrationBuilder.AddColumn<Guid>(
                name: "BaseRoomId",
                table: "LockerLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseLockerId",
                table: "FolderLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseFolderId",
                table: "DocumentLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LockerLogs_BaseRoomId",
                table: "LockerLogs",
                column: "BaseRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRequests_DocumentId",
                table: "ImportRequests",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FolderLogs_BaseLockerId",
                table: "FolderLogs",
                column: "BaseLockerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_BaseFolderId",
                table: "DocumentLogs",
                column: "BaseFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentLogs_Folders_BaseFolderId",
                table: "DocumentLogs",
                column: "BaseFolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FolderLogs_Lockers_BaseLockerId",
                table: "FolderLogs",
                column: "BaseLockerId",
                principalTable: "Lockers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LockerLogs_Rooms_BaseRoomId",
                table: "LockerLogs",
                column: "BaseRoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentLogs_Folders_BaseFolderId",
                table: "DocumentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_FolderLogs_Lockers_BaseLockerId",
                table: "FolderLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerLogs_Rooms_BaseRoomId",
                table: "LockerLogs");

            migrationBuilder.DropIndex(
                name: "IX_LockerLogs_BaseRoomId",
                table: "LockerLogs");

            migrationBuilder.DropIndex(
                name: "IX_ImportRequests_DocumentId",
                table: "ImportRequests");

            migrationBuilder.DropIndex(
                name: "IX_FolderLogs_BaseLockerId",
                table: "FolderLogs");

            migrationBuilder.DropIndex(
                name: "IX_DocumentLogs_BaseFolderId",
                table: "DocumentLogs");

            migrationBuilder.DropColumn(
                name: "BaseRoomId",
                table: "LockerLogs");

            migrationBuilder.DropColumn(
                name: "BaseLockerId",
                table: "FolderLogs");

            migrationBuilder.DropColumn(
                name: "BaseFolderId",
                table: "DocumentLogs");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "RequestLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ImportRequests_DocumentId",
                table: "ImportRequests",
                column: "DocumentId");
        }
    }
}
