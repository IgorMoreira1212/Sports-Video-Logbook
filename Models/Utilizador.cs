using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sports_Video_Logbook.Models
{
    public class Utilizador : IdentityUser
    {
        public DateTime DataCriacao { get; set; }
        public string? Numero_Mecanografico { get; set; }

        public ICollection<ProfessorUC> UCsLecionadas { get; set; } = new List<ProfessorUC>();
    }

    public class CreateUtilizadorViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? Numero_Mecanografico { get; set; }

        // Para alunos: seleção de turmas por UC
        public List<UCTurmasSelectionViewModel> UCsDisponiveis { get; set; } = new List<UCTurmasSelectionViewModel>();
        
        // IDs das turmas selecionadas (formato: "UCId_TurmaNome")
        public List<string> TurmasSelecionadas { get; set; } = new List<string>();
    }

    public class UCTurmasSelectionViewModel
    {
        public int UCId { get; set; }
        public string UCNome { get; set; } = string.Empty;
        public List<TurmaSelectionViewModel> Turmas { get; set; } = new List<TurmaSelectionViewModel>();
    }

    public class TurmaSelectionViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string ProfessorNome { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty; // formato: "UCId_TurmaNome"
    }
}
