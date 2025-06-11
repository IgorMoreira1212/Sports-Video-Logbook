using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports_Video_Logbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class cenasnovas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlunoId",
                table: "Tarefas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_AlunoId",
                table: "Tarefas",
                column: "AlunoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tarefas_AspNetUsers_AlunoId",
                table: "Tarefas",
                column: "AlunoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tarefas_AspNetUsers_AlunoId",
                table: "Tarefas");

            migrationBuilder.DropIndex(
                name: "IX_Tarefas_AlunoId",
                table: "Tarefas");

            migrationBuilder.DropColumn(
                name: "AlunoId",
                table: "Tarefas");
        }
    }
}
