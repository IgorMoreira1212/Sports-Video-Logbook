using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sports_Video_Logbook.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelUpdate05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UCs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UCs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfessoresUC",
                columns: table => new
                {
                    ProfessorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UCId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessoresUC", x => new { x.ProfessorId, x.UCId });
                    table.ForeignKey(
                        name: "FK_ProfessoresUC_AspNetUsers_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfessoresUC_UCs_UCId",
                        column: x => x.UCId,
                        principalTable: "UCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turmas",
                columns: table => new
                {
                    Nome = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UCId = table.Column<int>(type: "int", nullable: false),
                    ProfessorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turmas", x => new { x.Nome, x.UCId });
                    table.ForeignKey(
                        name: "FK_Turmas_AspNetUsers_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Turmas_UCs_UCId",
                        column: x => x.UCId,
                        principalTable: "UCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessoresUC_UCId",
                table: "ProfessoresUC",
                column: "UCId");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_ProfessorId",
                table: "Turmas",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Turmas_UCId",
                table: "Turmas",
                column: "UCId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfessoresUC");

            migrationBuilder.DropTable(
                name: "Turmas");

            migrationBuilder.DropTable(
                name: "UCs");
        }
    }
}
