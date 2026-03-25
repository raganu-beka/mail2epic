using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class UserGoogleAccountRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoogleMailboxAccounts_UserAccountId_EmailAddress",
                table: "GoogleMailboxAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleMailboxAccounts_UserAccountId",
                table: "GoogleMailboxAccounts",
                column: "UserAccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoogleMailboxAccounts_UserAccounts_UserAccountId",
                table: "GoogleMailboxAccounts",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoogleMailboxAccounts_UserAccounts_UserAccountId",
                table: "GoogleMailboxAccounts");

            migrationBuilder.DropIndex(
                name: "IX_GoogleMailboxAccounts_UserAccountId",
                table: "GoogleMailboxAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleMailboxAccounts_UserAccountId_EmailAddress",
                table: "GoogleMailboxAccounts",
                columns: new[] { "UserAccountId", "EmailAddress" },
                unique: true);
        }
    }
}
