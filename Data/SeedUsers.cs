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
                // Criar utilizador Admin
                var admin = new Utilizador
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    DataCriacao = DateTime.Now,
                };
                userManager.CreateAsync(admin, "Admin1234").Wait();
                userManager.AddToRoleAsync(admin, "Admin").Wait();

                // Criar utilizador Professor
                var professor = new Utilizador
                {
                    UserName = "Professor",
                    Email = "professor@gmail.com",
                    EmailConfirmed = true,
                    DataCriacao = DateTime.Now,
                };
                userManager.CreateAsync(professor, "Professor1234").Wait();
                userManager.AddToRoleAsync(professor, "Professor").Wait();

                // Criar utilizador Aluno
                var aluno = new Utilizador
                {
                    UserName = "Aluno",
                    Email = "aluno@gmail.com",
                    EmailConfirmed = true,
                    DataCriacao = DateTime.Now,
                    Numero_Mecanografico = "al00000"
                };
                userManager.CreateAsync(aluno, "Aluno1234").Wait();
                userManager.AddToRoleAsync(aluno, "Aluno").Wait();
            }
        }
    }
} 