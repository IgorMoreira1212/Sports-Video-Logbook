using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports_Video_Logbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InscricoesUC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UCId = table.Column<int>(type: "int", nullable: false),
                    TurmaNome = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscricoesUC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscricoesUC_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InscricoesUC_Turmas_TurmaNome_UCId",
                        columns: x => new { x.TurmaNome, x.UCId },
                        principalTable: "Turmas",
                        principalColumns: new[] { "Nome", "UCId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscricoesUC_UCs_UCId",
                        column: x => x.UCId,
                        principalTable: "UCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Concluida = table.Column<bool>(type: "bit", nullable: false),
                    ProfessorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TurmaNome = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TurmaUCId = table.Column<int>(type: "int", nullable: true),
                    UCId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tarefas_AspNetUsers_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tarefas_Turmas_TurmaNome_TurmaUCId",
                        columns: x => new { x.TurmaNome, x.TurmaUCId },
                        principalTable: "Turmas",
                        principalColumns: new[] { "Nome", "UCId" });
                    table.ForeignKey(
                        name: "FK_Tarefas_UCs_UCId",
                        column: x => x.UCId,
                        principalTable: "UCs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubmissoesTarefa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TarefaId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataSubmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotaFinal = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissoesTarefa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissoesTarefa_AspNetUsers_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubmissoesTarefa_Tarefas_TarefaId",
                        column: x => x.TarefaId,
                        principalTable: "Tarefas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TarefaSkills",
                columns: table => new
                {
                    TarefaId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Peso = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarefaSkills", x => new { x.TarefaId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_TarefaSkills_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TarefaSkills_Tarefas_TarefaId",
                        column: x => x.TarefaId,
                        principalTable: "Tarefas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacoesSkill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissaoTarefaId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesSkill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesSkill_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AvaliacoesSkill_SubmissoesTarefa_SubmissaoTarefaId",
                        column: x => x.SubmissaoTarefaId,
                        principalTable: "SubmissoesTarefa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesSkill_SkillId",
                table: "AvaliacoesSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesSkill_SubmissaoTarefaId",
                table: "AvaliacoesSkill",
                column: "SubmissaoTarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesUC_AlunoId_UCId",
                table: "InscricoesUC",
                columns: new[] { "AlunoId", "UCId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesUC_TurmaNome_UCId",
                table: "InscricoesUC",
                columns: new[] { "TurmaNome", "UCId" });

            migrationBuilder.CreateIndex(
                name: "IX_InscricoesUC_UCId",
                table: "InscricoesUC",
                column: "UCId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissoesTarefa_AlunoId",
                table: "SubmissoesTarefa",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissoesTarefa_TarefaId",
                table: "SubmissoesTarefa",
                column: "TarefaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_ProfessorId",
                table: "Tarefas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_TurmaNome_TurmaUCId",
                table: "Tarefas",
                columns: new[] { "TurmaNome", "TurmaUCId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_UCId",
                table: "Tarefas",
                column: "UCId");

            migrationBuilder.CreateIndex(
                name: "IX_TarefaSkills_SkillId",
                table: "TarefaSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesSkill");

            migrationBuilder.DropTable(
                name: "InscricoesUC");

            migrationBuilder.DropTable(
                name: "TarefaSkills");

            migrationBuilder.DropTable(
                name: "SubmissoesTarefa");

            migrationBuilder.DropTable(
                name: "Tarefas");
        }
    }
}
