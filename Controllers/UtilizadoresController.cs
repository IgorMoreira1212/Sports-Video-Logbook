using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_Video_Logbook.Data;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public UtilizadoresController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index(string search, string cargo, string error)
        {
            if (!string.IsNullOrEmpty(error))
                ViewBag.Error = error;
            var users = _userManager.Users.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }
            // Filtro por cargo (role)
            if (!string.IsNullOrEmpty(cargo) && cargo != "Todos")
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(cargo);
                users = users.Where(u => usersInRole.Select(x => x.Id).Contains(u.Id));
            }
            var userList = await users.ToListAsync();
            return View(userList);
        }

        // GET: Utilizadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Utilizadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Utilizador model, string password, string role)
        {
            if (ModelState.IsValid)
            {
                model.DataCriacao = DateTime.Now;
                var result = await _userManager.CreateAsync(model, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(model, role);
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // POST: Utilizadores/AlternarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarEstado(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                var allAdmins = await _userManager.GetUsersInRoleAsync("Admin");
                // Contar apenas admins ativos
                var activeAdmins = allAdmins.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now).ToList();
                if (activeAdmins.Count == 1 && (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.Now))
                {
                    return RedirectToAction(nameof(Index), new { error = "Não é possível desativar o único administrador ativo." });
                }
            }
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                return RedirectToAction(nameof(Index), new { error = "Não pode desativar a sua própria conta." });
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                // Ativar utilizador
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                // Desativar utilizador
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
} 