using Microsoft.AspNetCore.Identity;

namespace Sports_Video_Logbook.Models
{
    public class Utilizador : IdentityUser
    {
        public DateTime DataCriacao { get; set; }
        public string? Numero_Mecanografico { get; set; }
    }
}
