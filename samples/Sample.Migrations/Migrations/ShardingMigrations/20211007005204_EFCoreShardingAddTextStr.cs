using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Migrations.Migrations.ShardingMigrations
{
    public partial class EFCoreShardingAddTextStr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextStr",
                table: "ShardingWithMod",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextStr",
                table: "ShardingWithMod");
        }
    }
}
