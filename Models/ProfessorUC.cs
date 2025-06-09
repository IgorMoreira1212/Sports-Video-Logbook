using System.ComponentModel.DataAnnotations.Schema;

namespace Sports_Video_Logbook.Models
{
    public class ProfessorUC
    {
        public string ProfessorId { get; set; } = string.Empty;

        [ForeignKey("ProfessorId")]
        public Utilizador? Professor { get; set; }

        public int UCId { get; set; }

        [ForeignKey("UCId")]
        public UC? UC { get; set; }
    }
}
