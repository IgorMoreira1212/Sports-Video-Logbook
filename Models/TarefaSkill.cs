using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports_Video_Logbook.Models
{
    public class TarefaSkill
    {
        public int TarefaId { get; set; }
        public Tarefa? Tarefa { get; set; }

        public int SkillId { get; set; }
        public Skill? Skill { get; set; }

        [Range(0, 100)]
        public int Peso { get; set; } // Percentagem atribuída a esta skill
    }
}
