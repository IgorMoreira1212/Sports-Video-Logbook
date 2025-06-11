using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sports_Video_Logbook.Models
{
    public class UC
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public int NumeroTurmas { get; set; }

        public ICollection<ProfessorUC> ProfessoresUC { get; set; } = new List<ProfessorUC>();
    }

    public class CreateUCViewModel
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Range(1, 10, ErrorMessage = "O número de turmas deve estar entre 1 e 10")]
        public int NumeroTurmas { get; set; }

        public List<string> ProfessoresIds { get; set; } = new List<string>();
        
        public List<SelectListItem> ProfessoresDisponiveis { get; set; } = new List<SelectListItem>();
    }
}
