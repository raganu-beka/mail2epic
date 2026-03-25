using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAccountsAndChangeDateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleMailboxAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AllowedAccountId = table.Column<int>(type: "integer", nullable: false),
                    GoogleSubject = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EncryptedRefreshToken = table.Column<string>(type: "text", nullable: false),
                    EncryptedAccessToken = table.Column<string>(type: "text", nullable: true),
                    AccessTokenExpiration = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TokenType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleMailboxAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleMailboxAccounts_AllowedAccountId_EmailAddress",
                table: "GoogleMailboxAccounts",
                columns: new[] { "AllowedAccountId", "EmailAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleMailboxAccounts_GoogleSubject",
                table: "GoogleMailboxAccounts",
                column: "GoogleSubject",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleMailboxAccounts");
        }
    }
}
