using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Sports_Video_Logbook.Models
{
    public class Utilizador : IdentityUser
    {
        public DateTime DataCriacao { get; set; }
        public string? Numero_Mecanografico { get; set; }

        public ICollection<ProfessorUC> UCsLecionadas { get; set; } = new List<ProfessorUC>();
    }
}
