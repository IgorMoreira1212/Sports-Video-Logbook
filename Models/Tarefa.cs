using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sports_Video_Logbook.Models
{
    // Atributo de validação customizado para validar datas
    public class DataFimMaiorQueDataInicioAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var tarefa = (Tarefa)validationContext.ObjectInstance;
            
            if (tarefa.DataFim < tarefa.DataInicio)
            {
                return new ValidationResult("A data limite para submissão não pode ser anterior à data de início.");
            }
            
            return ValidationResult.Success;
        }
    }

    public class Tarefa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataInicio { get; set; }

        [Required]
        [DataFimMaiorQueDataInicio]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
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

        // Se for atribuída a um aluno específico
        public string? AlunoId { get; set; }
        public Utilizador? Aluno { get; set; }

        public ICollection<TarefaSkill> TarefaSkills { get; set; } = new List<TarefaSkill>();
        public ICollection<SubmissaoTarefa> Submissoes { get; set; } = new List<SubmissaoTarefa>();
    }

    // Atributo de validação para o ViewModel também
    public class DataFimViewModelMaiorQueDataInicioAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var viewModel = (CreateTarefaViewModel)validationContext.ObjectInstance;
            
            if (viewModel.DataFim < viewModel.DataInicio)
            {
                return new ValidationResult("A data limite para submissão não pode ser anterior à data de início.");
            }
            
            return ValidationResult.Success;
        }
    }

    public class CreateTarefaViewModel
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public int UCId { get; set; }

        public List<string> TurmasSelecionadas { get; set; } = new List<string>();

        // Para quando é um aluno específico (seleção única de turma)
        public string? TurmaSelecionada { get; set; }

        [Required]
        public string AtribuirA { get; set; } = string.Empty; // "Turma" ou "Aluno"

        public List<string> AlunosSelecionados { get; set; } = new List<string>();

        [Required]
        public List<int> SkillsSelecionadas { get; set; } = new List<int>();

        // Dicionário para armazenar as percentagens de cada skill (SkillId -> Peso)
        public Dictionary<int, int> SkillsPesos { get; set; } = new Dictionary<int, int>();

        [Required]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        [DataFimViewModelMaiorQueDataInicio]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataFim { get; set; } = DateTime.Now.AddDays(7);

        // Dados para popular dropdowns
        public List<UC> UCsDisponiveis { get; set; } = new List<UC>();
        public List<Turma> TurmasDisponiveis { get; set; } = new List<Turma>();
        public List<Skill> SkillsDisponiveis { get; set; } = new List<Skill>();
        public List<AlunoTurmaInfo> AlunosDisponiveis { get; set; } = new List<AlunoTurmaInfo>();
    }

    public class AlunoTurmaInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? NumeroMecanografico { get; set; }
        public string TurmaNome { get; set; } = string.Empty;
        public int UCId { get; set; }
        public string DisplayText => $"{UserName} ({NumeroMecanografico})";
    }
}
