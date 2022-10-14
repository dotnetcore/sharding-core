using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.SqlServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysTest",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysTest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysUserMod",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AgeGroup = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserMod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysUserSalary",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DateOfMonth = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<int>(type: "int", nullable: false),
                    SalaryLong = table.Column<long>(type: "bigint", nullable: false),
                    SalaryDecimal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalaryDouble = table.Column<double>(type: "float", nullable: false),
                    SalaryFloat = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUserSalary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestYearSharding",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreateTIme = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestYearSharding", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysTest");

            migrationBuilder.DropTable(
                name: "SysUserMod");

            migrationBuilder.DropTable(
                name: "SysUserSalary");

            migrationBuilder.DropTable(
                name: "TestYearSharding");
        }
    }
}
