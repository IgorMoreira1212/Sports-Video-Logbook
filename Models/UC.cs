using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Sports_Video_Logbook.Models
{
    public class UC
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public ICollection<ProfessorUC> ProfessoresUC { get; set; } = new List<ProfessorUC>();
    }
}
