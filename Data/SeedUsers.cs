using Microsoft.AspNetCore.Identity;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Data
{
    public class SeedUsers
    {
        public static void Seed(UserManager<Utilizador> userManager)
        {
            // Verifica se já existem utilizadores na base de dados
            if (userManager.Users.Any() == false)
            {
                // Criar apenas utilizador Admin
                var admin = new Utilizador
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    DataCriacao = DateTime.Now,
                };
                userManager.CreateAsync(admin, "Admin1234").Wait();
                userManager.AddToRoleAsync(admin, "Admin").Wait();
            }
        }
    }
} 