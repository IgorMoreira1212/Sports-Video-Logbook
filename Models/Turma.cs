using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports_Video_Logbook.Models
{
    public class Turma
    {
        [Key, Column(Order = 0)]
        public string Nome { get; set; } = string.Empty;

        [Key, Column(Order = 1)]
        public int UCId { get; set; }

        [ForeignKey("UCId")]
        public UC? UC { get; set; }

        [Required]
        public string ProfessorId { get; set; } = string.Empty;

        [ForeignKey("ProfessorId")]
        public Utilizador? Professor { get; set; }
    }
}
