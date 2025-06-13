using System.ComponentModel.DataAnnotations;

namespace Sports_Video_Logbook.Models
{
    public class AvaliacaoSkill
    {
        public int Id { get; set; }

        [Required]
        public int SubmissaoTarefaId { get; set; }
        public SubmissaoTarefa? SubmissaoTarefa { get; set; }

        [Required]
        public int SkillId { get; set; }
        public Skill? Skill { get; set; }

        [Range(0, 20)]
        public double Nota { get; set; }
    }
}
