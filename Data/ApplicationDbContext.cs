using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Sports_Video_Logbook.Models.Skill> Skill { get; set; } = default!;
    }
}
