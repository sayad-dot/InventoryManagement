using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class InventorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryAccesses_AspNetUsers_ApplicationUserId",
                table: "InventoryAccesses");

            migrationBuilder.DropIndex(
                name: "IX_InventoryAccesses_ApplicationUserId",
                table: "InventoryAccesses");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "InventoryAccesses");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Inventories",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomIdFormat",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Inventories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTags",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTags", x => new { x.InventoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_InventoryTags_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Office and technical equipment", "Equipment" },
                    { 2, "Office furniture and fixtures", "Furniture" },
                    { 3, "Books and publications", "Books" },
                    { 4, "Electronic devices and components", "Electronics" },
                    { 5, "Company vehicles and transportation", "Vehicles" },
                    { 6, "Tools and workshop equipment", "Tools" },
                    { 7, "Other categories", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "office" },
                    { 2, "technology" },
                    { 3, "furniture" },
                    { 4, "books" },
                    { 5, "electronics" },
                    { 6, "tools" },
                    { 7, "vehicles" },
                    { 8, "supplies" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTags_TagId",
                table: "InventoryTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Categories_CategoryId",
                table: "Inventories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Categories_CategoryId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "InventoryTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomIdFormat",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Inventories");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "InventoryAccesses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_ApplicationUserId",
                table: "InventoryAccesses",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryAccesses_AspNetUsers_ApplicationUserId",
                table: "InventoryAccesses",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
