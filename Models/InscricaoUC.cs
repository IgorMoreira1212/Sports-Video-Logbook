using Sports_Video_Logbook.Models;

public class InscricaoUC
{
    public int Id { get; set; } // Surrogate key

    public string AlunoId { get; set; } = string.Empty;
    public Utilizador? Aluno { get; set; }

    public int UCId { get; set; }
    public UC? UC { get; set; }

    public string TurmaNome { get; set; } = string.Empty;
    public Turma? Turma { get; set; }
}
