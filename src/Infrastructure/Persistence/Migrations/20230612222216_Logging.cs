using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Logging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Rooms",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Rooms",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Lockers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Lockers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Lockers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Lockers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Folders",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Folders",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Documents",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Documents",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "Created",
                table: "Borrows",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new NodaTime.LocalDateTime(1, 1, 1, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Borrows",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<LocalDateTime>(
                name: "LastModified",
                table: "Borrows",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifiedBy",
                table: "Borrows",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Time = table.Column<LocalDateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentLogs_Documents_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Documents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FolderLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Time = table.Column<LocalDateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderLogs_Folders_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FolderLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Time = table.Column<LocalDateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockerLogs_Lockers_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Lockers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LockerLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    Time = table.Column<LocalDateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomLogs_Rooms_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_ObjectId",
                table: "DocumentLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLogs_UserId",
                table: "DocumentLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderLogs_ObjectId",
                table: "FolderLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FolderLogs_UserId",
                table: "FolderLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLogs_ObjectId",
                table: "LockerLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLogs_UserId",
                table: "LockerLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomLogs_ObjectId",
                table: "RoomLogs",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomLogs_UserId",
                table: "RoomLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentLogs");

            migrationBuilder.DropTable(
                name: "FolderLogs");

            migrationBuilder.DropTable(
                name: "LockerLogs");

            migrationBuilder.DropTable(
                name: "RoomLogs");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Borrows");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Borrows");
        }
    }
}
