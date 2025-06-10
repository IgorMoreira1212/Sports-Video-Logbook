using System.ComponentModel.DataAnnotations;

namespace Sports_Video_Logbook.Models
{
    public class Tarefa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Required]
        public bool Concluida { get; set; }

        [Required]
        public string ProfessorId { get; set; } = string.Empty;
        public Utilizador? Professor { get; set; }

        // Se for atribuída a uma turma específica
        public string? TurmaNome { get; set; }
        public int? TurmaUCId { get; set; }
        public Turma? Turma { get; set; }

        // Se for atribuída a todas as turmas de uma UC
        public int? UCId { get; set; }
        public UC? UC { get; set; }

        public ICollection<TarefaSkill> TarefaSkills { get; set; } = new List<TarefaSkill>();
        public ICollection<SubmissaoTarefa> Submissoes { get; set; } = new List<SubmissaoTarefa>();
    }
}
