﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blue.Mail2Epic.Migrations
{
    /// <inheritdoc />
    public partial class FixCreatedByColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "AllowedAccounts"
                ALTER COLUMN "CreatedBy" TYPE integer
                USING 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedBy",
                table: "AllowedAccounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
