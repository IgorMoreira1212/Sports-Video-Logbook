using System.ComponentModel.DataAnnotations;

namespace Sports_Video_Logbook.Models
{
    public class SubmissaoTarefa
    {
        public int Id { get; set; }

        [Required]
        public int TarefaId { get; set; }
        public Tarefa? Tarefa { get; set; }

        [Required]
        public string AlunoId { get; set; } = string.Empty;
        public Utilizador? Aluno { get; set; }

        public string? Texto { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime DataSubmissao { get; set; }

        public ICollection<AvaliacaoSkill> AvaliacoesSkills { get; set; } = new List<AvaliacaoSkill>();

        public double? NotaFinal { get; set; } // Calculada após avaliação
    }
}
