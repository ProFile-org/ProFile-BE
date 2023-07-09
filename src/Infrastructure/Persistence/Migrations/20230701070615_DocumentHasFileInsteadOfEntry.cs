using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentHasFileInsteadOfEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Entries_EntryId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Memberships");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.RenameColumn(
                name: "EntryId",
                table: "Documents",
                newName: "FileId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_EntryId",
                table: "Documents",
                newName: "IX_Documents_FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Files_FileId",
                table: "Documents",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Files_FileId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Documents",
                newName: "EntryId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_FileId",
                table: "Documents",
                newName: "IX_Documents_EntryId");

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                    table.UniqueConstraint("AK_UserGroups_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Memberships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memberships", x => new { x.UserId, x.UserGroupId });
                    table.ForeignKey(
                        name: "FK_Memberships_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Memberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_UserGroupId",
                table: "Memberships",
                column: "UserGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Entries_EntryId",
                table: "Documents",
                column: "EntryId",
                principalTable: "Entries",
                principalColumn: "Id");
        }
    }
}
