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
using Microsoft.AspNetCore.Http;
using System.IO;

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

        // GET: Tarefas/Index - Para professores: tarefas criadas | Para alunos: LogBook (tarefas concluídas/expiradas)
        public async Task<IActionResult> Index(string search, string uc)
        {
            // Verificar se o usuário está logado
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var query = _context.Tarefas
                .Include(t => t.Professor)
                .Include(t => t.Aluno)
                .Include(t => t.UC)
                .Include(t => t.TarefaSkills)
                .ThenInclude(ts => ts.Skill)
                .AsQueryable();

            List<dynamic> ucsList;

            if (User.IsInRole("Professor"))
            {
                // Para professores: mostrar todas as tarefas criadas por eles
                query = query.Where(t => t.ProfessorId == currentUser.Id);
                
                // Carregar UCs disponíveis (todas as UCs)
                ucsList = await _context.UCs.OrderBy(u => u.Nome).ToListAsync<dynamic>();
            }
            else if (User.IsInRole("Aluno"))
            {
                // Para alunos: LogBook - apenas tarefas CONCLUÍDAS e EXPIRADAS
                query = query.Where(t => t.AlunoId == currentUser.Id && 
                               (t.Concluida || (!t.Concluida && t.DataFim < DateTime.Now)));
                
                // Ordenar por data de conclusão/expiração (mais recentes primeiro)
                query = query.OrderByDescending(t => t.Concluida).ThenByDescending(t => t.DataFim);
                
                // Carregar UCs disponíveis (apenas das tarefas do logbook)
                ucsList = await _context.Tarefas
                    .Include(t => t.UC)
                    .Where(t => t.AlunoId == currentUser.Id && 
                               (t.Concluida || (!t.Concluida && t.DataFim < DateTime.Now)))
                    .Select(t => t.UC)
                    .Distinct()
                    .Where(uc => uc != null)
                    .OrderBy(uc => uc.Nome)
                    .ToListAsync<dynamic>();
            }
            else
            {
                return Forbid();
            }

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

            ViewBag.UCsDisponiveis = ucsList;
            ViewBag.SearchValue = search;
            ViewBag.UCValue = uc;

            return View(await query.ToListAsync());
        }

        // GET: Tarefas/MinhasTarefas - Para alunos verem suas tarefas atribuídas
        public async Task<IActionResult> MinhasTarefas(string search, string uc)
        {
            // Verificar se o usuário está logado
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Verificar se o usuário é um aluno
            if (!User.IsInRole("Aluno"))
            {
                return Forbid();
            }

            // Obter apenas tarefas PENDENTES OU EXPIRADAS atribuídas ao aluno atual
            // (não concluídas)
            var query = _context.Tarefas
                .Include(t => t.Professor)
                .Include(t => t.UC)
                .Include(t => t.TarefaSkills)
                .ThenInclude(ts => ts.Skill)
                .Where(t => t.AlunoId == currentUser.Id && 
                           !t.Concluida)
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

            // Ordenar por data de fim (mais próximas primeiro)
            query = query.OrderBy(t => t.DataFim);

            // Carregar dados para o dropdown de UC (apenas das tarefas não concluídas)
            var ucsList = await _context.Tarefas
                .Include(t => t.UC)
                .Where(t => t.AlunoId == currentUser.Id && 
                           !t.Concluida)
                .Select(t => t.UC)
                .Distinct()
                .Where(uc => uc != null)
                .OrderBy(uc => uc.Nome)
                .ToListAsync();
            
            ViewBag.UCsDisponiveis = ucsList;
            ViewBag.SearchValue = search;
            ViewBag.UCValue = uc;

            return View(await query.ToListAsync());
        }

        // POST: Tarefas/CompletarTarefa/5 - Para marcar tarefa como concluída
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarTarefa(int id)
        {
            // Verificar se o usuário está logado
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Verificar se o usuário é um aluno
            if (!User.IsInRole("Aluno"))
            {
                return Forbid();
            }

            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null)
            {
                return NotFound();
            }

            // Verificar se a tarefa pertence ao aluno atual
            if (tarefa.AlunoId != currentUser.Id)
            {
                return Forbid();
            }

            // Marcar tarefa como concluída
            tarefa.Concluida = true;
            _context.Update(tarefa);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tarefa marcada como concluída!";
            return RedirectToAction(nameof(MinhasTarefas));
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

            var tarefa = await _context.Tarefas
                .Include(t => t.Aluno)
                .Include(t => t.Professor)
                .Include(t => t.UC)
                .FirstOrDefaultAsync(t => t.Id == id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descricao,DataInicio,DataFim,Concluida,ProfessorId,TurmaNome,UCId,AlunoId,TurmaUCId")] Tarefa tarefa)
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
                var tituloTarefa = tarefa.Titulo;
                _context.Tarefas.Remove(tarefa);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"A tarefa \"{tituloTarefa}\" foi removida com sucesso.";
            }
            else
            {
                TempData["Error"] = "Tarefa não encontrada.";
            }

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

        // GET: Tarefas/Submeter/5
        public async Task<IActionResult> Submeter(int id)
        {
            var tarefa = await _context.Tarefas.FindAsync(id);
            if (tarefa == null) return NotFound();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || tarefa.AlunoId != currentUser.Id) return Forbid();
            var vm = new SubmissaoTarefaViewModel { TarefaId = id };
            return View(vm);
        }

        // POST: Tarefas/Submeter/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submeter(SubmissaoTarefaViewModel vm)
        {
            var tarefa = await _context.Tarefas.FindAsync(vm.TarefaId);
            var currentUser = await _userManager.GetUserAsync(User);
            if (tarefa == null || currentUser == null || tarefa.AlunoId != currentUser.Id) return Forbid();
            if (!ModelState.IsValid) return View(vm);

            var submissao = new SubmissaoTarefa
            {
                TarefaId = vm.TarefaId,
                AlunoId = currentUser.Id,
                Texto = vm.Texto,
                DataSubmissao = DateTime.Now
            };
            _context.Add(submissao);
            await _context.SaveChangesAsync();

            // Guardar ficheiros
            if (vm.Ficheiros != null && vm.Ficheiros.Count > 0)
            {
                var uploadDir = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                foreach (var file in vm.Ficheiros)
                {
                    if (file.Length > 0)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();
                        var tipo = ext == ".jpg" || ext == ".jpeg" || ext == ".png" ? "imagem" :
                                   ext == ".mp4" || ext == ".mov" ? "video" : "documento";
                        var fileName = $"submissao_{submissao.Id}_{Guid.NewGuid()}{ext}";
                        var path = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        _context.Add(new SubmissaoFicheiro
                        {
                            SubmissaoTarefaId = submissao.Id,
                            Caminho = "/uploads/" + fileName,
                            Tipo = tipo
                        });
                    }
                }
            }
            // Marcar tarefa como concluída e guardar tudo
            tarefa.Concluida = true;
            _context.Update(tarefa);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Submissão efetuada com sucesso!";
            return RedirectToAction("MinhasTarefas");
        }

        // GET: Tarefas/VerSubmissoes/5
        public async Task<IActionResult> VerSubmissoes(int id)
        {
            var tarefa = await _context.Tarefas.Include(t => t.UC).FirstOrDefaultAsync(t => t.Id == id);
            var currentUser = await _userManager.GetUserAsync(User);
            if (tarefa == null || currentUser == null || tarefa.ProfessorId != currentUser.Id) return Forbid();
            var submissoes = await _context.Set<SubmissaoTarefa>()
                .Include(s => s.Aluno)
                .Include(s => s.Ficheiros)
                .Where(s => s.TarefaId == id)
                .OrderByDescending(s => s.DataSubmissao)
                .ToListAsync();
            ViewBag.Tarefa = tarefa;
            return View(submissoes);
        }
    }
}
