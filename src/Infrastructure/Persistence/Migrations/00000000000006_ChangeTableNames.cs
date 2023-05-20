using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhysicalDomainEntitiesToDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrow_Document_DocumentId",
                table: "Borrow");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrow_Users_BorrowerId",
                table: "Borrow");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Departments_DepartmentId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Folder_FolderId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Users_ImporterId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Folder_Locker_LockerId",
                table: "Folder");

            migrationBuilder.DropForeignKey(
                name: "FK_Locker_Rooms_RoomId",
                table: "Locker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locker",
                table: "Locker");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Folder",
                table: "Folder");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Document",
                table: "Document");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Borrow",
                table: "Borrow");

            migrationBuilder.RenameTable(
                name: "Locker",
                newName: "Lockers");

            migrationBuilder.RenameTable(
                name: "Folder",
                newName: "Folders");

            migrationBuilder.RenameTable(
                name: "Document",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "Borrow",
                newName: "Borrows");

            migrationBuilder.RenameIndex(
                name: "IX_Locker_RoomId",
                table: "Lockers",
                newName: "IX_Lockers_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Folder_LockerId",
                table: "Folders",
                newName: "IX_Folders_LockerId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_ImporterId",
                table: "Documents",
                newName: "IX_Documents_ImporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_FolderId",
                table: "Documents",
                newName: "IX_Documents_FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_DepartmentId",
                table: "Documents",
                newName: "IX_Documents_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrow_DocumentId",
                table: "Borrows",
                newName: "IX_Borrows_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrow_BorrowerId",
                table: "Borrows",
                newName: "IX_Borrows_BorrowerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lockers",
                table: "Lockers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Folders",
                table: "Folders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Borrows",
                table: "Borrows",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrows_Documents_DocumentId",
                table: "Borrows",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrows_Users_BorrowerId",
                table: "Borrows",
                column: "BorrowerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Departments_DepartmentId",
                table: "Documents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Users_ImporterId",
                table: "Documents",
                column: "ImporterId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_Lockers_LockerId",
                table: "Folders",
                column: "LockerId",
                principalTable: "Lockers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_Rooms_RoomId",
                table: "Lockers",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrows_Documents_DocumentId",
                table: "Borrows");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrows_Users_BorrowerId",
                table: "Borrows");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Departments_DepartmentId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Folders_FolderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Users_ImporterId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Folders_Lockers_LockerId",
                table: "Folders");

            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_Rooms_RoomId",
                table: "Lockers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lockers",
                table: "Lockers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Folders",
                table: "Folders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Borrows",
                table: "Borrows");

            migrationBuilder.RenameTable(
                name: "Lockers",
                newName: "Locker");

            migrationBuilder.RenameTable(
                name: "Folders",
                newName: "Folder");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Document");

            migrationBuilder.RenameTable(
                name: "Borrows",
                newName: "Borrow");

            migrationBuilder.RenameIndex(
                name: "IX_Lockers_RoomId",
                table: "Locker",
                newName: "IX_Locker_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Folders_LockerId",
                table: "Folder",
                newName: "IX_Folder_LockerId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ImporterId",
                table: "Document",
                newName: "IX_Document_ImporterId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_FolderId",
                table: "Document",
                newName: "IX_Document_FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_DepartmentId",
                table: "Document",
                newName: "IX_Document_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrows_DocumentId",
                table: "Borrow",
                newName: "IX_Borrow_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Borrows_BorrowerId",
                table: "Borrow",
                newName: "IX_Borrow_BorrowerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locker",
                table: "Locker",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Folder",
                table: "Folder",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Document",
                table: "Document",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Borrow",
                table: "Borrow",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrow_Document_DocumentId",
                table: "Borrow",
                column: "DocumentId",
                principalTable: "Document",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrow_Users_BorrowerId",
                table: "Borrow",
                column: "BorrowerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Departments_DepartmentId",
                table: "Document",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Folder_FolderId",
                table: "Document",
                column: "FolderId",
                principalTable: "Folder",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Users_ImporterId",
                table: "Document",
                column: "ImporterId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Folder_Locker_LockerId",
                table: "Folder",
                column: "LockerId",
                principalTable: "Locker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locker_Rooms_RoomId",
                table: "Locker",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
