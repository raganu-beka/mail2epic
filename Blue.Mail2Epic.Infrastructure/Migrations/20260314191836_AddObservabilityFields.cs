using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class AddObservabilityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpicConfidence",
                table: "EmailMappings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageSenderAddress",
                table: "EmailMappings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MessageSubject",
                table: "EmailMappings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reasoning",
                table: "EmailMappings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailMappings_UserAccounts_UserAccountId",
                table: "EmailMappings");

            migrationBuilder.DropIndex(
                name: "IX_EmailMappings_UserAccountId",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "EpicConfidence",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "MessageSenderAddress",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "MessageSubject",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "Reasoning",
                table: "EmailMappings");

            migrationBuilder.DropColumn(
                name: "UserAccountId",
                table: "EmailMappings");
        }
    }
}
