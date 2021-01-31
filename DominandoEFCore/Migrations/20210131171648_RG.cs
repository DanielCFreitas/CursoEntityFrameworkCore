using Microsoft.EntityFrameworkCore.Migrations;

namespace DominandoEFCore.Migrations
{
    public partial class RG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RG",
                table: "Funcionarios",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RG",
                table: "Funcionarios");
        }
    }
}
