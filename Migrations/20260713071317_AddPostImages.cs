using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaoVatWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddPostImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "Posts",
                newName: "IsActive");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Posts",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Posts",
                newName: "IsFeatured");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Posts",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Posts",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Posts",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RejectReason",
                table: "Posts",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Posts",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
