using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Migrations.Migrations.ShardingMigrations
{
    public partial class EFCoreSharding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoShardingTable",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoShardingTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShardingWithDateTime",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShardingWithDateTime", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShardingWithMod",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShardingWithMod", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoShardingTable");

            migrationBuilder.DropTable(
                name: "ShardingWithDateTime");

            migrationBuilder.DropTable(
                name: "ShardingWithMod");
        }
    }
}
