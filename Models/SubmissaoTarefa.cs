using System;
using System.Collections.Generic;
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
        public DateTime DataSubmissao { get; set; } = DateTime.Now;

        public ICollection<AvaliacaoSkill> AvaliacoesSkills { get; set; } = new List<AvaliacaoSkill>();
        public ICollection<SubmissaoFicheiro> Ficheiros { get; set; } = new List<SubmissaoFicheiro>();

        public double? NotaFinal { get; set; } // Calculada após avaliação
    }

    public class SubmissaoFicheiro
    {
        public int Id { get; set; }
        public int SubmissaoTarefaId { get; set; }
        public SubmissaoTarefa SubmissaoTarefa { get; set; }
        public string Caminho { get; set; }
        public string Tipo { get; set; } // "imagem", "video", "documento"
    }
}
