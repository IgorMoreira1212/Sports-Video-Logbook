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
    public class TarefasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public TarefasController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tarefas
        public async Task<IActionResult> Index(string search, string uc)
        {
            // Obter o utilizador atual
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }
            
            // Filtrar apenas tarefas criadas pelo utilizador atual
            var query = _context.Tarefas
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .Include(t => t.UC)
                .Where(t => t.ProfessorId == currentUser.Id)
                .AsQueryable();
            
            // Aplicar filtro de pesquisa por título
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Titulo.Contains(search));
            }
            
            // Aplicar filtro por UC
            if (!string.IsNullOrEmpty(uc) && int.TryParse(uc, out int ucId))
            {
                query = query.Where(t => t.UCId == ucId);
            }
            
            // Carregar dados para os dropdowns de filtro
            ViewBag.UCsDisponiveis = await _context.UCs.OrderBy(u => u.Nome).ToListAsync();
            ViewBag.TurmasDisponiveis = await _context.Turmas.OrderBy(t => t.Nome).ToListAsync();
            
            return View(await query.ToListAsync());
        }

        // GET: Tarefas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarefa = await _context.Tarefas
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .Include(t => t.UC)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tarefa == null)
            {
                return NotFound();
            }

            return View(tarefa);
        }

        // GET: Tarefas/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateTarefaViewModel();
            await CarregarDadosFormulario(viewModel);
            return View(viewModel);
        }

        // POST: Tarefas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTarefaViewModel viewModel)
        {
            // Garantir que a DataInicio seja sempre o momento atual
            viewModel.DataInicio = DateTime.Now;
            
            // Se não foi fornecida uma DataFim, definir para 7 dias após a criação
            if (viewModel.DataFim == default(DateTime) || viewModel.DataFim <= viewModel.DataInicio)
            {
                viewModel.DataFim = viewModel.DataInicio.AddDays(7);
            }
            
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                // Validação específica
                if (viewModel.AtribuirA == "Aluno")
                {
                    if (!viewModel.AlunosSelecionados.Any())
                    {
                        ModelState.AddModelError("AlunosSelecionados", "Deve selecionar pelo menos um aluno quando atribuir a alunos específicos");
                    }
                    if (!viewModel.TurmasSelecionadas.Any())
                    {
                        ModelState.AddModelError("TurmasSelecionadas", "Deve selecionar pelo menos uma turma quando atribuir a alunos específicos");
                    }
                }

                if (viewModel.AtribuirA == "Turma" && (!viewModel.TurmasSelecionadas.Any()))
                {
                    ModelState.AddModelError("TurmasSelecionadas", "Deve selecionar pelo menos uma turma quando atribuir a turmas específicas");
                }

                if (ModelState.IsValid)
                {
                    // Atribuição baseada na escolha
                    if (viewModel.AtribuirA == "Turma")
                    {
                        // Para múltiplas turmas, criar uma tarefa para cada aluno de cada turma selecionada
                        foreach (var turmaNome in viewModel.TurmasSelecionadas)
                        {
                            // Obter todos os alunos inscritos nesta turma
                            var alunosNaTurma = await _context.InscricoesUC
                                .Include(i => i.Aluno)
                                .Where(i => i.TurmaNome == turmaNome && i.UCId == viewModel.UCId)
                                .Select(i => i.Aluno)
                                .ToListAsync();

                            // Filtrar apenas alunos com role "Aluno"
                            var alunosRole = await _userManager.GetUsersInRoleAsync("Aluno");
                            var alunosValidosNaTurma = alunosNaTurma.Where(a => alunosRole.Contains(a)).ToList();

                            // Criar uma tarefa para cada aluno da turma
                            foreach (var aluno in alunosValidosNaTurma)
                            {
                                var tarefaAluno = new Tarefa
                                {
                                    Titulo = viewModel.Nome,
                                    Descricao = viewModel.Descricao,
                                    DataInicio = viewModel.DataInicio,
                                    DataFim = viewModel.DataFim,
                                    Concluida = false,
                                    ProfessorId = currentUser.Id,
                                    AlunoId = aluno.Id,
                                    TurmaNome = turmaNome,
                                    TurmaUCId = viewModel.UCId,
                                    UCId = viewModel.UCId
                                };

                                _context.Add(tarefaAluno);
                                await _context.SaveChangesAsync();

                                // Associar skills à tarefa
                                foreach (var skillId in viewModel.SkillsSelecionadas)
                                {
                                    var tarefaSkill = new TarefaSkill
                                    {
                                        TarefaId = tarefaAluno.Id,
                                        SkillId = skillId
                                    };
                                    _context.TarefaSkills.Add(tarefaSkill);
                                }
                            }
                        }
                    }
                    else if (viewModel.AtribuirA == "Aluno")
                    {
                        // Para múltiplos alunos, criar uma tarefa para cada aluno selecionado
                        foreach (var alunoId in viewModel.AlunosSelecionados)
                        {
                            // Obter a turma do aluno nesta UC (das turmas selecionadas)
                            var inscricaoAluno = await _context.InscricoesUC
                                .FirstOrDefaultAsync(i => i.AlunoId == alunoId && i.UCId == viewModel.UCId && 
                                                         viewModel.TurmasSelecionadas.Contains(i.TurmaNome));

                            if (inscricaoAluno != null)
                            {
                                var tarefaAluno = new Tarefa
                                {
                                    Titulo = viewModel.Nome,
                                    Descricao = viewModel.Descricao,
                                    DataInicio = viewModel.DataInicio,
                                    DataFim = viewModel.DataFim,
                                    Concluida = false,
                                    ProfessorId = currentUser.Id,
                                    AlunoId = alunoId,
                                    TurmaNome = inscricaoAluno.TurmaNome,
                                    TurmaUCId = viewModel.UCId,
                                    UCId = viewModel.UCId
                                };

                                _context.Add(tarefaAluno);
                                await _context.SaveChangesAsync();

                                // Associar skills à tarefa
                                foreach (var skillId in viewModel.SkillsSelecionadas)
                                {
                                    var tarefaSkill = new TarefaSkill
                                    {
                                        TarefaId = tarefaAluno.Id,
                                        SkillId = skillId
                                    };
                                    _context.TarefaSkills.Add(tarefaSkill);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Atribuir a toda a UC - criar tarefa para todos os alunos da UC
                        var todosAlunosUC = await _context.InscricoesUC
                            .Include(i => i.Aluno)
                            .Where(i => i.UCId == viewModel.UCId)
                            .Select(i => i.Aluno)
                            .Distinct()
                            .ToListAsync();

                        // Filtrar apenas alunos com role "Aluno"
                        var alunosRole = await _userManager.GetUsersInRoleAsync("Aluno");
                        var alunosValidosUC = todosAlunosUC.Where(a => alunosRole.Contains(a)).ToList();

                        // Criar uma tarefa para cada aluno da UC
                        foreach (var aluno in alunosValidosUC)
                        {
                            // Obter a turma do aluno nesta UC
                            var inscricaoAluno = await _context.InscricoesUC
                                .FirstOrDefaultAsync(i => i.AlunoId == aluno.Id && i.UCId == viewModel.UCId);

                            var tarefaAlunoUC = new Tarefa
                            {
                                Titulo = viewModel.Nome,
                                Descricao = viewModel.Descricao,
                                DataInicio = viewModel.DataInicio,
                                DataFim = viewModel.DataFim,
                                Concluida = false,
                                ProfessorId = currentUser.Id,
                                AlunoId = aluno.Id,
                                TurmaNome = inscricaoAluno?.TurmaNome,
                                TurmaUCId = viewModel.UCId,
                                UCId = viewModel.UCId
                            };

                            _context.Add(tarefaAlunoUC);
                            await _context.SaveChangesAsync();

                            // Associar skills à tarefa
                            foreach (var skillId in viewModel.SkillsSelecionadas)
                            {
                                var tarefaSkill = new TarefaSkill
                                {
                                    TarefaId = tarefaAlunoUC.Id,
                                    SkillId = skillId
                                };
                                _context.TarefaSkills.Add(tarefaSkill);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            await CarregarDadosFormulario(viewModel);
            return View(viewModel);
        }

        // GET: Tarefas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }
            
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            ViewData["ProfessorId"] = new SelectList(professores.OrderBy(p => p.UserName), "Id", "UserName", tarefa.ProfessorId);
            ViewData["TurmaNome"] = new SelectList(_context.Turmas, "Nome", "Nome", tarefa.TurmaNome);
            ViewData["UCId"] = new SelectList(_context.UCs, "Id", "Nome", tarefa.UCId);
            return View(tarefa);
        }

        // POST: Tarefas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descricao,DataInicio,DataFim,Concluida,ProfessorId,TurmaNome,UCId")] Tarefa tarefa)
        {
            if (id != tarefa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tarefa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TarefaExists(tarefa.Id))
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
            
            var professores = await _userManager.GetUsersInRoleAsync("Professor");
            ViewData["ProfessorId"] = new SelectList(professores.OrderBy(p => p.UserName), "Id", "UserName", tarefa.ProfessorId);
            ViewData["TurmaNome"] = new SelectList(_context.Turmas, "Nome", "Nome", tarefa.TurmaNome);
            ViewData["UCId"] = new SelectList(_context.UCs, "Id", "Nome", tarefa.UCId);
            return View(tarefa);
        }

        // GET: Tarefas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tarefa = await _context.Tarefas
                .Include(t => t.Professor)
                .Include(t => t.Turma)
                .Include(t => t.UC)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tarefa == null)
            {
                return NotFound();
            }

            return View(tarefa);
        }

        // POST: Tarefas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa != null)
            {
                _context.Tarefas.Remove(tarefa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TarefaExists(int id)
        {
            return _context.Tarefas.Any(e => e.Id == id);
        }

        private async Task CarregarDadosFormulario(CreateTarefaViewModel viewModel)
        {
            viewModel.UCsDisponiveis = await _context.UCs.OrderBy(u => u.Nome).ToListAsync();
            viewModel.TurmasDisponiveis = await _context.Turmas
                .Include(t => t.UC)
                .OrderBy(t => t.UC.Nome)
                .ThenBy(t => t.Nome)
                .ToListAsync();
            viewModel.SkillsDisponiveis = await _context.Skill.OrderBy(s => s.Nome).ToListAsync();
            
            // Carregar alunos com informações das turmas
            viewModel.AlunosDisponiveis = await CarregarAlunosPorTurmas();
        }

        private async Task<List<AlunoTurmaInfo>> CarregarAlunosPorTurmas()
        {
            var alunosInfo = new List<AlunoTurmaInfo>();
            
            var inscricoes = await _context.InscricoesUC
                .Include(i => i.Aluno)
                .Include(i => i.Turma)
                .Include(i => i.Turma.UC)
                .Where(i => i.Aluno != null)
                .ToListAsync();
            
            var alunos = await _userManager.GetUsersInRoleAsync("Aluno");
            
            foreach (var inscricao in inscricoes)
            {
                if (alunos.Contains(inscricao.Aluno))
                {
                    alunosInfo.Add(new AlunoTurmaInfo
                    {
                        Id = inscricao.Aluno.Id,
                        UserName = inscricao.Aluno.UserName,
                        NumeroMecanografico = inscricao.Aluno.Numero_Mecanografico,
                        TurmaNome = inscricao.TurmaNome,
                        UCId = inscricao.Turma.UCId
                    });
                }
            }
            
            return alunosInfo.OrderBy(a => a.UserName).ToList();
        }
    }
}
