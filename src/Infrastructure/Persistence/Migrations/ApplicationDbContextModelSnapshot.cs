﻿// <auto-generated />
using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Entities.Department", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("Domain.Entities.Digital.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FileId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("FileId")
                        .IsUnique();

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("Domain.Entities.Digital.FileEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<byte[]>("FileData")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("FileType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Domain.Entities.Digital.UserGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("UserGroups");
                });

            modelBuilder.Entity("Domain.Entities.Logging.DocumentLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ObjectId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("UserId");

                    b.ToTable("DocumentLogs");
                });

            modelBuilder.Entity("Domain.Entities.Logging.FolderLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ObjectId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("UserId");

                    b.ToTable("FolderLogs");
                });

            modelBuilder.Entity("Domain.Entities.Logging.LockerLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ObjectId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("UserId");

                    b.ToTable("LockerLogs");
                });

            modelBuilder.Entity("Domain.Entities.Logging.RoomLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ObjectId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Time")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId");

                    b.HasIndex("UserId");

                    b.ToTable("RoomLogs");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Borrow", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("ActualReturnTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<LocalDateTime>("BorrowTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("BorrowerId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("DocumentId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("DueTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BorrowerId");

                    b.HasIndex("DocumentId");

                    b.ToTable("Borrows");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Document", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("DepartmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("DocumentType")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<Guid?>("EntryId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FolderId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ImporterId")
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("EntryId")
                        .IsUnique();

                    b.HasIndex("FolderId");

                    b.HasIndex("ImporterId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Folder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Capacity")
                        .HasColumnType("integer");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("LockerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("NumberOfDocuments")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LockerId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Locker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Capacity")
                        .HasColumnType("integer");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("NumberOfFolders")
                        .HasColumnType("integer");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoomId");

                    b.ToTable("Lockers");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Capacity")
                        .HasColumnType("integer");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("DepartmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("boolean");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("NumberOfLockers")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.HasIndex("DepartmentId")
                        .IsUnique();

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Staff", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("UserId");

                    b.Property<Guid?>("RoomId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoomId")
                        .IsUnique();

                    b.ToTable("Staffs");
                });

            modelBuilder.Entity("Domain.Entities.RefreshToken", b =>
                {
                    b.Property<Guid>("Token")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("CreationDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<LocalDateTime>("ExpiryDateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsInvalidated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsUsed")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("JwtId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Token");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("Domain.Entities.ResetPasswordToken", b =>
                {
                    b.Property<string>("TokenHash")
                        .HasColumnType("text");

                    b.Property<LocalDateTime>("ExpirationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsInvalidated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("TokenHash");

                    b.HasIndex("UserId");

                    b.ToTable("ResetPasswordTokens");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<LocalDateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("DepartmentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<bool>("IsActivated")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<LocalDateTime?>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid?>("LastModifiedBy")
                        .HasColumnType("uuid");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("Position")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Memberships", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserGroupId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "UserGroupId");

                    b.HasIndex("UserGroupId");

                    b.ToTable("Memberships");
                });

            modelBuilder.Entity("Domain.Entities.Digital.Entry", b =>
                {
                    b.HasOne("Domain.Entities.Digital.FileEntity", "File")
                        .WithOne()
                        .HasForeignKey("Domain.Entities.Digital.Entry", "FileId");

                    b.Navigation("File");
                });

            modelBuilder.Entity("Domain.Entities.Logging.DocumentLog", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Document", "Object")
                        .WithMany()
                        .HasForeignKey("ObjectId");

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Object");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Logging.FolderLog", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Folder", "Object")
                        .WithMany()
                        .HasForeignKey("ObjectId");

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Object");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Logging.LockerLog", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Locker", "Object")
                        .WithMany()
                        .HasForeignKey("ObjectId");

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Object");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Logging.RoomLog", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Room", "Object")
                        .WithMany()
                        .HasForeignKey("ObjectId");

                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Object");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Borrow", b =>
                {
                    b.HasOne("Domain.Entities.User", "Borrower")
                        .WithMany()
                        .HasForeignKey("BorrowerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Physical.Document", "Document")
                        .WithMany()
                        .HasForeignKey("DocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Borrower");

                    b.Navigation("Document");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Document", b =>
                {
                    b.HasOne("Domain.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.HasOne("Domain.Entities.Digital.Entry", "Entry")
                        .WithOne()
                        .HasForeignKey("Domain.Entities.Physical.Document", "EntryId");

                    b.HasOne("Domain.Entities.Physical.Folder", "Folder")
                        .WithMany("Documents")
                        .HasForeignKey("FolderId");

                    b.HasOne("Domain.Entities.User", "Importer")
                        .WithMany()
                        .HasForeignKey("ImporterId");

                    b.Navigation("Department");

                    b.Navigation("Entry");

                    b.Navigation("Folder");

                    b.Navigation("Importer");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Folder", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Locker", "Locker")
                        .WithMany("Folders")
                        .HasForeignKey("LockerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Locker");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Locker", b =>
                {
                    b.HasOne("Domain.Entities.Physical.Room", "Room")
                        .WithMany("Lockers")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Room");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Room", b =>
                {
                    b.HasOne("Domain.Entities.Department", "Department")
                        .WithOne("Room")
                        .HasForeignKey("Domain.Entities.Physical.Room", "DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Staff", b =>
                {
                    b.HasOne("Domain.Entities.User", "User")
                        .WithOne()
                        .HasForeignKey("Domain.Entities.Physical.Staff", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.Physical.Room", "Room")
                        .WithOne("Staff")
                        .HasForeignKey("Domain.Entities.Physical.Staff", "RoomId");

                    b.Navigation("Room");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.RefreshToken", b =>
                {
                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.ResetPasswordToken", b =>
                {
                    b.HasOne("Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Entities.User", b =>
                {
                    b.HasOne("Domain.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.Navigation("Department");
                });

            modelBuilder.Entity("Memberships", b =>
                {
                    b.HasOne("Domain.Entities.Digital.UserGroup", null)
                        .WithMany()
                        .HasForeignKey("UserGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Entities.Department", b =>
                {
                    b.Navigation("Room");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Folder", b =>
                {
                    b.Navigation("Documents");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Locker", b =>
                {
                    b.Navigation("Folders");
                });

            modelBuilder.Entity("Domain.Entities.Physical.Room", b =>
                {
                    b.Navigation("Lockers");

                    b.Navigation("Staff");
                });
#pragma warning restore 612, 618
        }
    }
}
