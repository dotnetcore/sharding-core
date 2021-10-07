using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Migrations.Migrations.ShardingMigrations
{
    public partial class EFCoreShardingAddTextStr1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextStr1",
                table: "ShardingWithMod",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "123");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextStr1",
                table: "ShardingWithMod");
        }
    }
}
