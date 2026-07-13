using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaoVatWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVipTablesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessages_Posts_PostId",
                table: "ContactMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactMessages",
                table: "ContactMessages");

            migrationBuilder.RenameTable(
                name: "ContactMessages",
                newName: "ContactMessage");

            migrationBuilder.RenameIndex(
                name: "IX_ContactMessages_PostId",
                table: "ContactMessage",
                newName: "IX_ContactMessage_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage",
                column: "ContactMessageId");

            migrationBuilder.CreateTable(
                name: "VipPackages",
                columns: table => new
                {
                    VipPackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    MaxPosts = table.Column<int>(type: "int", nullable: false),
                    CanHighlightPost = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VipPackages", x => x.VipPackageId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VipOrders",
                columns: table => new
                {
                    VipOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VipPackageId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VipExpiredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VipOrders", x => x.VipOrderId);
                    table.ForeignKey(
                        name: "FK_VipOrders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VipOrders_VipPackages_VipPackageId",
                        column: x => x.VipPackageId,
                        principalTable: "VipPackages",
                        principalColumn: "VipPackageId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VipOrders_UserId",
                table: "VipOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VipOrders_VipPackageId",
                table: "VipOrders",
                column: "VipPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessage_Posts_PostId",
                table: "ContactMessage",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactMessage_Posts_PostId",
                table: "ContactMessage");

            migrationBuilder.DropTable(
                name: "VipOrders");

            migrationBuilder.DropTable(
                name: "VipPackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactMessage",
                table: "ContactMessage");

            migrationBuilder.RenameTable(
                name: "ContactMessage",
                newName: "ContactMessages");

            migrationBuilder.RenameIndex(
                name: "IX_ContactMessage_PostId",
                table: "ContactMessages",
                newName: "IX_ContactMessages_PostId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactMessages",
                table: "ContactMessages",
                column: "ContactMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactMessages_Posts_PostId",
                table: "ContactMessages",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "PostId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
