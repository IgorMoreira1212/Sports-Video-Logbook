using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports_Video_Logbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class onemore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissoesTarefa_Tarefas_TarefaId",
                table: "SubmissoesTarefa");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissoesTarefa_Tarefas_TarefaId",
                table: "SubmissoesTarefa",
                column: "TarefaId",
                principalTable: "Tarefas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissoesTarefa_Tarefas_TarefaId",
                table: "SubmissoesTarefa");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissoesTarefa_Tarefas_TarefaId",
                table: "SubmissoesTarefa",
                column: "TarefaId",
                principalTable: "Tarefas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
