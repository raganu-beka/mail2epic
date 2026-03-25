using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisplayNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "UserAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "UserAccounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
