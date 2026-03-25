using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class MultipleEmailRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailMappings_UserAccounts_UserAccountId",
                table: "EmailMappings");

            migrationBuilder.DropIndex(
                name: "IX_EmailMappings_UserAccountId",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "EmailMappings");

            migrationBuilder.CreateTable(
                name: "EmailMappingRecipients",
                columns: table => new
                {
                    EmailMappingId = table.Column<int>(type: "integer", nullable: false),
                    UserAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMappingRecipients", x => new { x.EmailMappingId, x.UserAccountId });
                    table.ForeignKey(
                        name: "FK_EmailMappingRecipients_EmailMappings_EmailMappingId",
                        column: x => x.EmailMappingId,
                        principalTable: "EmailMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailMappingRecipients_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMappingRecipients_UserAccountId",
                table: "EmailMappingRecipients",
                column: "UserAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailMappingRecipients");

            migrationBuilder.AddColumn<int>(
                name: "UserAccountId",
                table: "EmailMappings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmailMappings_UserAccountId",
                table: "EmailMappings",
                column: "UserAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMappings_UserAccounts_UserAccountId",
                table: "EmailMappings",
                column: "UserAccountId",
                principalTable: "UserAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
