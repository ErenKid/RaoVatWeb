using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaoVatWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixAreaParentNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentArea",
                table: "Areas");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Areas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ParentAreaId",
                table: "Areas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Areas_ParentAreaId",
                table: "Areas",
                column: "ParentAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Areas_ParentAreaId",
                table: "Areas",
                column: "ParentAreaId",
                principalTable: "Areas",
                principalColumn: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Areas_ParentAreaId",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_ParentAreaId",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "ParentAreaId",
                table: "Areas");

            migrationBuilder.AddColumn<string>(
                name: "ParentArea",
                table: "Areas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
