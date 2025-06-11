using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Sports_Video_Logbook.Data;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Controllers
{
    public class UCsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public UCsController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UCs
        public async Task<IActionResult> Index()
        {
            var ucs = await _context.UCs
                .Select(uc => new
                {
                    UC = uc,
                    NumeroTurmas = _context.Turmas.Count(t => t.UCId == uc.Id)
                })
                .ToListAsync();

            return View(ucs);
        }

        // GET: UCs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uC = await _context.UCs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uC == null)
            {
                return NotFound();
            }

            return View(uC);
        }

        // GET: UCs/Create
        public async Task<IActionResult> Create()
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            
            var viewModel = new CreateUCViewModel
            {
                ProfessoresDisponiveis = professores
                    .Where(p => !string.IsNullOrEmpty(p.UserName) && !string.IsNullOrEmpty(p.Email))
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id,
                        Text = $"{p.UserName} ({p.Email})"
                    }).ToList()
            };
            
            // Debug: verificar se temos professores
            System.Diagnostics.Debug.WriteLine($"Found {professores.Count} professors in role");
            System.Diagnostics.Debug.WriteLine($"Valid professors for select: {viewModel.ProfessoresDisponiveis.Count}");
            
            return View(viewModel);
        }

        // POST: UCs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUCViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Verificar se temos professores suficientes
                if (viewModel.ProfessoresIds.Count < viewModel.NumeroTurmas)
                {
                    ModelState.AddModelError("", "Deve selecionar pelo menos um professor para cada turma.");
                    await CarregarProfessores(viewModel);
                    return View(viewModel);
                }

                // Criar a UC
                var uc = new UC
                {
                    Nome = viewModel.Nome,
                    NumeroTurmas = viewModel.NumeroTurmas
                };
                
                _context.Add(uc);
                await _context.SaveChangesAsync();

                // Criar as turmas com professores atribuídos
                for (int i = 0; i < viewModel.NumeroTurmas; i++)
                {
                    var professorId = viewModel.ProfessoresIds[i % viewModel.ProfessoresIds.Count];
                    
                    var turma = new Turma
                    {
                        Nome = $"Turma {i + 1}",
                        UCId = uc.Id,
                        ProfessorId = professorId
                    };
                    _context.Turmas.Add(turma);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            await CarregarProfessores(viewModel);
            return View(viewModel);
        }

        private async Task CarregarProfessores(CreateUCViewModel viewModel)
        {
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            viewModel.ProfessoresDisponiveis = professores
                .Where(p => !string.IsNullOrEmpty(p.UserName) && !string.IsNullOrEmpty(p.Email))
                .Select(p => new SelectListItem
                {
                    Value = p.Id,
                    Text = $"{p.UserName} ({p.Email})"
                }).ToList();
                
            System.Diagnostics.Debug.WriteLine($"CarregarProfessores: Found {professores.Count} professors, {viewModel.ProfessoresDisponiveis.Count} valid");
        }

        // GET: UCs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uC = await _context.UCs.FindAsync(id);
            if (uC == null)
            {
                return NotFound();
            }
            return View(uC);
        }

        // POST: UCs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] UC uC)
        {
            if (id != uC.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uC);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UCExists(uC.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(uC);
        }

        // GET: UCs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uC = await _context.UCs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (uC == null)
            {
                return NotFound();
            }

            return View(uC);
        }

        // POST: UCs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uC = await _context.UCs.FindAsync(id);
            if (uC != null)
            {
                _context.UCs.Remove(uC);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UCExists(int id)
        {
            return _context.UCs.Any(e => e.Id == id);
        }
    }
}
