using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomFieldsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to safely add columns only if they don't exist
            
            // Add Items table columns safely
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- CustomId
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomId') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomId"" text NULL;
                    END IF;
                    
                    -- String fields
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomString1Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomString1Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomString2Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomString2Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomString3Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomString3Value"" text NULL;
                    END IF;
                    
                    -- Text fields
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomText1Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomText1Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomText2Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomText2Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomText3Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomText3Value"" text NULL;
                    END IF;
                    
                    -- Number fields
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomNumber1Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomNumber1Value"" numeric NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomNumber2Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomNumber2Value"" numeric NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomNumber3Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomNumber3Value"" numeric NULL;
                    END IF;
                    
                    -- Boolean fields
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomBool1Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomBool1Value"" boolean NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomBool2Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomBool2Value"" boolean NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomBool3Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomBool3Value"" boolean NULL;
                    END IF;
                    
                    -- File fields
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomFile1Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomFile1Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomFile2Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomFile2Value"" text NULL;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='CustomFile3Value') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""CustomFile3Value"" text NULL;
                    END IF;
                    
                    -- Version field
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Items' AND column_name='Version') THEN
                        ALTER TABLE ""Items"" ADD COLUMN ""Version"" integer NOT NULL DEFAULT 1;
                    END IF;
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomText3Value",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool1Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomBool1Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool2Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomBool2Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomBool3Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomBool3Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomFile1Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomFile1Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomFile2Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomFile2Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomFile3Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomFile3Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomNumber1Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomNumber1Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomNumber2Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomNumber2Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomNumber3Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomNumber3Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString1Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString1Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString2Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString2Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString3Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomString3Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText1Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText1Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText2Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText2Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText3Active",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomText3Name",
                table: "Inventories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldOrder",
                table: "Inventories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomBool1Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomBool2Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomBool3Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomFile1Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomFile2Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomFile3Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomNumber1Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomNumber2Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomNumber3Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomString1Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomString2Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomString3Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomText1Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomText2Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomText3Value",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "CustomBool1Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool1Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool2Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool2Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool3Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomBool3Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile1Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile1Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile2Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile2Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile3Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomFile3Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber1Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber1Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber2Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber2Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber3Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomNumber3Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString1Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString1Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString2Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString2Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString3Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomString3Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText1Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText1Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText2Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText2Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText3Active",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CustomText3Name",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "FieldOrder",
                table: "Inventories");
        }
    }
}
