using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAccounts");

            migrationBuilder.DropTable(
                name: "AllowedAccounts");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "GoogleMailboxAccounts",
                newName: "Scope");

            migrationBuilder.RenameColumn(
                name: "AllowedAccountId",
                table: "GoogleMailboxAccounts",
                newName: "UserAccountId");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpiration",
                table: "GoogleMailboxAccounts",
                newName: "AccessTokenExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_GoogleMailboxAccounts_AllowedAccountId_EmailAddress",
                table: "GoogleMailboxAccounts",
                newName: "IX_GoogleMailboxAccounts_UserAccountId_EmailAddress");

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccounts_Email",
                table: "UserAccounts",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccounts");

            migrationBuilder.RenameColumn(
                name: "UserAccountId",
                table: "GoogleMailboxAccounts",
                newName: "AllowedAccountId");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "GoogleMailboxAccounts",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "AccessTokenExpiresAt",
                table: "GoogleMailboxAccounts",
                newName: "AccessTokenExpiration");

            migrationBuilder.RenameIndex(
                name: "IX_GoogleMailboxAccounts_UserAccountId_EmailAddress",
                table: "GoogleMailboxAccounts",
                newName: "IX_GoogleMailboxAccounts_AllowedAccountId_EmailAddress");

            migrationBuilder.CreateTable(
                name: "AdminAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAccounts_Username",
                table: "AdminAccounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllowedAccounts_Email",
                table: "AllowedAccounts",
                column: "Email",
                unique: true);
        }
    }
}
