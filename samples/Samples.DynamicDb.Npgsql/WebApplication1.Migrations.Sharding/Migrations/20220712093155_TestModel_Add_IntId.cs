using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Migrations.Sharding.Migrations
{
    public partial class TestModel_Add_IntId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TestModels",
                table: "TestModels");

            migrationBuilder.DropColumn(
                name: "Id2",
                table: "TestModels");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TestModels",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestModels",
                table: "TestModels",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TestModels",
                table: "TestModels");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TestModels");

            migrationBuilder.AddColumn<Guid>(
                name: "Id2",
                table: "TestModels",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestModels",
                table: "TestModels",
                column: "Id2");
        }
    }
}
