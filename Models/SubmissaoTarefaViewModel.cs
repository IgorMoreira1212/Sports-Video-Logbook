using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sports_Video_Logbook.Models
{
    public class SubmissaoTarefaViewModel
    {
        public int TarefaId { get; set; }
        [Required]
        [Display(Name = "Texto da Submissão")]
        public string Texto { get; set; }
        public List<IFormFile> Ficheiros { get; set; } = new();
    }
} 