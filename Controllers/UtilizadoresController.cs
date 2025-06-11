using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_Video_Logbook.Data;
using Sports_Video_Logbook.Models;
using System.Collections.Generic;

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
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateUtilizadorViewModel();
            await CarregarUCsETurmas(viewModel);
            return View(viewModel);
        }

        // POST: Utilizadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUtilizadorViewModel viewModel)
        {
            // Debug: Log os valores recebidos
            System.Diagnostics.Debug.WriteLine($"Creating user: {viewModel.UserName}, Role: {viewModel.Role}, Email: {viewModel.Email}");
            
            // Validação básica manual
            if (string.IsNullOrEmpty(viewModel.UserName))
            {
                ModelState.AddModelError("UserName", "UserName é obrigatório");
            }
            
            if (string.IsNullOrEmpty(viewModel.Email))
            {
                ModelState.AddModelError("Email", "Email é obrigatório");
            }
            
            if (string.IsNullOrEmpty(viewModel.Password))
            {
                ModelState.AddModelError("Password", "Password é obrigatória");
            }
            
            if (string.IsNullOrEmpty(viewModel.Role))
            {
                ModelState.AddModelError("Role", "Role é obrigatório");
            }
            
            // Verificar se o role existe
            if (viewModel.Role != "Admin" && viewModel.Role != "Professor" && viewModel.Role != "Aluno")
            {
                ModelState.AddModelError("Role", $"Role '{viewModel.Role}' não é válido");
            }
            
            // Validação específica para alunos
            if (viewModel.Role == "Aluno")
            {
                if (string.IsNullOrEmpty(viewModel.Numero_Mecanografico))
                {
                    ModelState.AddModelError("Numero_Mecanografico", "Número mecanográfico é obrigatório para alunos");
                }
                
                if (!viewModel.TurmasSelecionadas.Any())
                {
                    ModelState.AddModelError("TurmasSelecionadas", "Deve selecionar pelo menos uma turma para o aluno");
                }
            }
            
            if (ModelState.IsValid)
            {
                var utilizador = new Utilizador
                {
                    UserName = viewModel.UserName,
                    Email = viewModel.Email,
                    DataCriacao = DateTime.Now
                };
                
                // Limpar o Numero_Mecanografico se não for aluno
                if (viewModel.Role == "Aluno")
                {
                    utilizador.Numero_Mecanografico = viewModel.Numero_Mecanografico;
                }
                
                try
                {
                    var result = await _userManager.CreateAsync(utilizador, viewModel.Password);
                    if (result.Succeeded)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(utilizador, viewModel.Role);
                        if (roleResult.Succeeded)
                        {
                            // Se for um aluno, inscrever nas turmas selecionadas
                            if (viewModel.Role == "Aluno")
                            {
                                await InscriverAlunoEmTurmasSelecionadas(utilizador.Id, viewModel.TurmasSelecionadas);
                            }
                            
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                System.Diagnostics.Debug.WriteLine($"Role Error: {error.Code} - {error.Description}");
                                ModelState.AddModelError(string.Empty, $"Erro ao atribuir role: {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Identity Error: {error.Code} - {error.Description}");
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError(string.Empty, $"Erro inesperado: {ex.Message}");
                }
            }
            else
            {
                // Debug: Log validation errors
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error in {modelError.Key}: {error.ErrorMessage}");
                    }
                }
            }
            
            // Recarregar UCs e turmas em caso de erro
            await CarregarUCsETurmas(viewModel);
            return View(viewModel);
        }

        private async Task CarregarUCsETurmas(CreateUtilizadorViewModel viewModel)
        {
            var ucs = await _context.UCs.Include(uc => uc.ProfessoresUC).ToListAsync();
            
            viewModel.UCsDisponiveis = new List<UCTurmasSelectionViewModel>();
            
            foreach (var uc in ucs)
            {
                var turmas = await _context.Turmas
                    .Where(t => t.UCId == uc.Id)
                    .Include(t => t.Professor)
                    .ToListAsync();
                
                if (turmas.Any())
                {
                    var ucViewModel = new UCTurmasSelectionViewModel
                    {
                        UCId = uc.Id,
                        UCNome = uc.Nome,
                        Turmas = turmas.Select(t => new TurmaSelectionViewModel
                        {
                            Nome = t.Nome,
                            ProfessorNome = t.Professor?.UserName ?? "Sem professor",
                            Value = $"{uc.Id}_{t.Nome}"
                        }).ToList()
                    };
                    
                    viewModel.UCsDisponiveis.Add(ucViewModel);
                }
            }
        }

        private async Task InscriverAlunoEmTurmasSelecionadas(string alunoId, List<string> turmasSelecionadas)
        {
            foreach (var turmaValue in turmasSelecionadas)
            {
                // Formato: "UCId_TurmaNome"
                var parts = turmaValue.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[0], out int ucId))
                {
                    var turmaNome = parts[1];
                    
                    // Verificar se já não está inscrito nesta UC
                    var jaInscrito = await _context.InscricoesUC
                        .AnyAsync(i => i.AlunoId == alunoId && i.UCId == ucId);
                    
                    if (!jaInscrito)
                    {
                        var inscricao = new InscricaoUC
                        {
                            AlunoId = alunoId,
                            UCId = ucId,
                            TurmaNome = turmaNome
                        };
                        
                        _context.InscricoesUC.Add(inscricao);
                    }
                }
            }
            
            await _context.SaveChangesAsync();
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