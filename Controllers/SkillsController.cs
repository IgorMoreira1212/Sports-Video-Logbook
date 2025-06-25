using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sports_Video_Logbook.Data;
using Sports_Video_Logbook.Models;
using System.IO;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Interactive;

namespace Sports_Video_Logbook.Controllers
{
    [Authorize]
    public class SkillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public SkillsController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Skills
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Para admins: mostrar todas as skills
                return View(await _context.Skill.ToListAsync());
            }
            else if (User.IsInRole("Aluno"))
            {
                // Para alunos: mostrar apenas skills com avaliação >= 9.5
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                var skillsComAvaliacaoPositiva = await _context.AvaliacoesSkill
                    .Include(a => a.Skill)
                    .Include(a => a.SubmissaoTarefa)
                    .ThenInclude(s => s.Tarefa)
                    .Where(a => a.SubmissaoTarefa.Tarefa.AlunoId == currentUser.Id && a.Nota >= 9.5)
                    .Select(a => a.Skill)
                    .Distinct()
                    .OrderBy(s => s.Nome)
                    .ToListAsync();

                // Buscar todas as avaliações do aluno para cada skill
                var avaliacoesDoAluno = await _context.AvaliacoesSkill
                    .Include(a => a.Skill)
                    .Include(a => a.SubmissaoTarefa)
                    .ThenInclude(s => s.Tarefa)
                    .Where(a => a.SubmissaoTarefa.Tarefa.AlunoId == currentUser.Id)
                    .ToListAsync();

                // Agrupar por skill
                var avaliacoesPorSkill = avaliacoesDoAluno
                    .GroupBy(a => a.SkillId)
                    .ToDictionary(g => g.Key, g => g.Select(a => a.Nota).ToList());

                ViewBag.AvaliacoesSkill = avaliacoesPorSkill;

                return View(skillsComAvaliacaoPositiva);
            }
            else
            {
                return Forbid();
            }
        }

        // GET: Skills/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skill
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // GET: Skills/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Skill skill)
        {
            if (ModelState.IsValid)
            {
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skill.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }
            return View(skill);
        }

        // POST: Skills/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Skill skill)
        {
            if (id != skill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skill.Id))
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
            return View(skill);
        }

        // GET: Skills/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skill
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // POST: Skills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _context.Skill.FindAsync(id);
            if (skill != null)
            {
                _context.Skill.Remove(skill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Skills/GerarCertificado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> GerarCertificado(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var skill = await _context.Skill.FindAsync(id);
            if (skill == null) return NotFound();

            // Buscar a melhor nota do aluno para a skill
            var melhorNota = await _context.AvaliacoesSkill
                .Where(a => a.SkillId == id && a.SubmissaoTarefa.Tarefa.AlunoId == currentUser.Id)
                .MaxAsync(a => (double?)a.Nota) ?? 0.0;

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "Certificado.pdf");
            var outputStream = new MemoryStream();

            using (var templateStream = System.IO.File.OpenRead(templatePath))
            {
                // Carregar o PDF
                using (var loadedDocument = new PdfLoadedDocument(templateStream))
                {
                    // Acessar o formulário do PDF
                    var form = loadedDocument.Form;
                    if (form != null)
                    {
                        // Preencher os campos
                        if (form.Fields["Nome do Aluno"] is PdfLoadedTextBoxField nomeAlunoField)
                            nomeAlunoField.Text = currentUser.UserName;
                        if (form.Fields["nome da skill"] is PdfLoadedTextBoxField nomeSkillField)
                            nomeSkillField.Text = skill.Nome;
                        if (form.Fields["nota"] is PdfLoadedTextBoxField notaField)
                            notaField.Text = melhorNota.ToString("F1");
                    }
                    // Salvar o PDF preenchido
                    loadedDocument.Save(outputStream);
                }
            }
            outputStream.Position = 0;
            return File(outputStream.ToArray(), "application/pdf", $"Certificado_{skill.Nome}_{currentUser.UserName}.pdf");
        }

        private bool SkillExists(int id)
        {
            return _context.Skill.Any(e => e.Id == id);
        }
    }
}
