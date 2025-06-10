using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sports_Video_Logbook.Models;

namespace Sports_Video_Logbook.Data
{
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Skill> Skill { get; set; } = default!;
        public DbSet<UC> UCs { get; set; } = default!;
        public DbSet<ProfessorUC> ProfessoresUC { get; set; } = default!;
        public DbSet<Turma> Turmas { get; set; } = default!;
        public DbSet<InscricaoUC> InscricoesUC { get; set; } = default!;
        public DbSet<Tarefa> Tarefas { get; set; } = default!;
        public DbSet<TarefaSkill> TarefaSkills { get; set; } = default!;
        public DbSet<SubmissaoTarefa> SubmissoesTarefa { get; set; } = default!;
        public DbSet<AvaliacaoSkill> AvaliacoesSkill { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chave composta para ProfessorUC
            modelBuilder.Entity<ProfessorUC>()
                .HasKey(puc => new { puc.ProfessorId, puc.UCId });

            // Chave composta para Turma
            modelBuilder.Entity<Turma>()
                .HasKey(t => new { t.Nome, t.UCId });

            // Surrogate key for InscricaoUC
            modelBuilder.Entity<InscricaoUC>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<InscricaoUC>()
                .HasIndex(i => new { i.AlunoId, i.UCId })
                .IsUnique();

            // Relação composta para Turma em InscricaoUC (Restrict to avoid cascade path)
            modelBuilder.Entity<InscricaoUC>()
                .HasOne(i => i.Turma)
                .WithMany()
                .HasForeignKey(i => new { i.TurmaNome, i.UCId })
                .OnDelete(DeleteBehavior.Restrict);

            // Chave composta para TarefaSkill
            modelBuilder.Entity<TarefaSkill>()
                .HasKey(ts => new { ts.TarefaId, ts.SkillId });

            // Relação entre Tarefa e Professor
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.Professor)
                .WithMany()
                .HasForeignKey(t => t.ProfessorId);

            // Relação entre Tarefa e Turma (opcional)
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.Turma)
                .WithMany()
                .HasForeignKey(t => new { t.TurmaNome, t.TurmaUCId })
                .IsRequired(false);

            // Relação entre Tarefa e UC (opcional)
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.UC)
                .WithMany()
                .HasForeignKey(t => t.UCId)
                .IsRequired(false);

            // Relação entre SubmissaoTarefa e Tarefa
            modelBuilder.Entity<SubmissaoTarefa>()
                .HasOne(st => st.Tarefa)
                .WithMany(t => t.Submissoes)
                .HasForeignKey(st => st.TarefaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação entre SubmissaoTarefa e Aluno
            modelBuilder.Entity<SubmissaoTarefa>()
                .HasOne(st => st.Aluno)
                .WithMany()
                .HasForeignKey(st => st.AlunoId);

            // Relação entre AvaliacaoSkill e SubmissaoTarefa
            modelBuilder.Entity<AvaliacaoSkill>()
                .HasOne(a => a.SubmissaoTarefa)
                .WithMany(st => st.AvaliacoesSkills)
                .HasForeignKey(a => a.SubmissaoTarefaId);

            // Relação entre AvaliacaoSkill e Skill
            modelBuilder.Entity<AvaliacaoSkill>()
                .HasOne(a => a.Skill)
                .WithMany()
                .HasForeignKey(a => a.SkillId);
        }
    }
}
