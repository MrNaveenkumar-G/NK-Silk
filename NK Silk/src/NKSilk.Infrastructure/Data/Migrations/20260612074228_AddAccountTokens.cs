using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NKSilk.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetExpiresUtc",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordResetExpiresUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Customers");
        }
    }
}
