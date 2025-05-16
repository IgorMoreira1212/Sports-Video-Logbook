using Microsoft.AspNetCore.Identity;

namespace Sports_Video_Logbook.Data
{
    public class SeedRoles
    {
        public static void Seed(RoleManager<IdentityRole>roleManager)
        {
            if (roleManager.Roles.Any() == false)
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
                roleManager.CreateAsync(new IdentityRole("Professor")).Wait();
                roleManager.CreateAsync(new IdentityRole("Aluno")).Wait();
            }
        }
    }
}
