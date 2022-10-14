using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.SqlServer.Migrations
{
    public partial class InitialCreateAddName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SysTest",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "SysTest");
        }
    }
}
