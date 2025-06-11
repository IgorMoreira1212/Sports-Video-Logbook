using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sports_Video_Logbook.Data;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Controllers
{
    public class UCsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UCsController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: UCs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome")] UC uC, int NumeroTurmas)
        {
            if (ModelState.IsValid && NumeroTurmas > 0)
            {
                // Adicionar a UC
                _context.Add(uC);
                await _context.SaveChangesAsync();

                // Criar as turmas baseadas no número especificado
                for (int i = 1; i <= NumeroTurmas; i++)
                {
                    var turma = new Turma
                    {
                        Nome = $"Turma {i}",
                        UCId = uC.Id,
                        ProfessorId = "" // Será definido posteriormente
                    };
                    _context.Turmas.Add(turma);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Se NumeroTurmas não foi fornecido, adicionar erro ao ModelState
            if (NumeroTurmas <= 0)
            {
                ModelState.AddModelError("NumeroTurmas", "Deve especificar o número de turmas.");
            }
            
            return View(uC);
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
