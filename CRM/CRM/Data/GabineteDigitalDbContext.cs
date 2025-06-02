using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class GabineteDigitalDbContext : IdentityDbContext<MembroEquipe, IdentityRole<Guid>, Guid>
    {
        public GabineteDigitalDbContext(DbContextOptions<GabineteDigitalDbContext> options)
            : base(options)
        {
        }

        public DbSet<Solicitacao> Solicitacoes { get; set; }
        public DbSet<Cidadao> Cidadaos { get; set; }
        public DbSet<Anexo> Anexos { get; set; }
        public DbSet<Observacao> Observacoes { get; set; }
        public DbSet<Compromisso> Compromissos { get; set; }
        public DbSet<HistoricoSolicitacao> HistoricoSolicitacoes { get; set; }
        public DbSet<ProjetoVereador> ProjetosVereador { get; set; }
        public DbSet<TagSolicitacao> TagSolicitacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamentos para Solicitacao
            modelBuilder.Entity<Solicitacao>()
                .HasOne(s => s.Cidadao)
                .WithMany(c => c.Solicitacoes)
                .HasForeignKey(s => s.CidadaoId)
                .IsRequired(false); // Cidadao pode ser nulo (ex: solicitação interna)

            modelBuilder.Entity<Solicitacao>()
                .HasOne(s => s.MembroAtribuido)
                .WithMany() // MembroEquipe não tem coleção de solicitações atribuídas
                .HasForeignKey(s => s.MembroAtribuidoId)
                .IsRequired(false);

            modelBuilder.Entity<Solicitacao>()
                .HasMany(s => s.Anexos)
                .WithOne(a => a.Solicitacao)
                .HasForeignKey(a => a.SolicitacaoId);

            modelBuilder.Entity<Solicitacao>()
                .HasMany(s => s.Observacoes)
                .WithOne(o => o.Solicitacao)
                .HasForeignKey(o => o.SolicitacaoId);

            modelBuilder.Entity<Solicitacao>()
                .HasMany(s => s.Tags)
                .WithOne(t => t.Solicitacao)
                .HasForeignKey(t => t.SolicitacaoId);

            // Mapeamentos para Compromisso
            modelBuilder.Entity<Compromisso>()
                .HasOne(c => c.Cidadao)
                .WithMany()
                .HasForeignKey(c => c.CidadaoId)
                .IsRequired(false);

            modelBuilder.Entity<Compromisso>()
                .HasOne(c => c.Responsavel)
                .WithMany()
                .HasForeignKey(c => c.ResponsavelId)
                .IsRequired(false);

            // Mapeamentos para Observacao
            modelBuilder.Entity<Observacao>()
                .HasOne(o => o.Autor)
                .WithMany()
                .HasForeignKey(o => o.AutorId);

            // Mapeamentos para HistoricoSolicitacao
            modelBuilder.Entity<HistoricoSolicitacao>()
                .HasOne(h => h.Solicitacao)
                .WithMany()
                .HasForeignKey(h => h.SolicitacaoId);

            modelBuilder.Entity<HistoricoSolicitacao>()
                .HasOne(h => h.UsuarioResponsavel)
                .WithMany()
                .HasForeignKey(h => h.UsuarioResponsavelId);

            // Configurar Enum para string no DB (opcional, mas bom para legibilidade)
            modelBuilder.Entity<Solicitacao>()
                .Property(s => s.Categoria)
                .HasConversion<string>();
            modelBuilder.Entity<Solicitacao>()
                .Property(s => s.Prioridade)
                .HasConversion<string>();
            modelBuilder.Entity<Solicitacao>()
                .Property(s => s.Status)
                .HasConversion<string>();
            modelBuilder.Entity<Compromisso>()
                .Property(c => c.TipoAtendimento)
                .HasConversion<string>();
            modelBuilder.Entity<Compromisso>()
                .Property(c => c.Status)
                .HasConversion<string>();
            modelBuilder.Entity<HistoricoSolicitacao>()
                .Property(h => h.TipoEvento)
                .HasConversion<string>();
        }
    }