using Microsoft.EntityFrameworkCore;
using EDSG.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EDSG.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Construtor padrão para design-time
        public AppDbContext() { }

        // Tabelas existentes
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Favorito> Favoritos { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }

        // Tabelas novas para Serviços Profissionais
        public DbSet<ServicoProfissional> ServicosProfissionais { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CONFIGURAÇÃO DAS RELAÇÕES - CORRIGIDA
            // ============================================

            // Configuração das relações para ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ServicosComoCliente)
                .WithOne(s => s.Cliente)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ServicosComoProfissional)
                .WithOne(s => s.Profissional)
                .HasForeignKey(s => s.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.AvaliacoesRecebidas)
                .WithOne(a => a.Avaliado)
                .HasForeignKey(a => a.AvaliadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.AvaliacoesEnviadas)
                .WithOne(a => a.Avaliador)
                .HasForeignKey(a => a.AvaliadorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.MensagensEnviadas)
                .WithOne(m => m.Remetente)
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.MensagensRecebidas)
                .WithOne(m => m.Destinatario)
                .HasForeignKey(m => m.DestinatarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.FavoritadoPor)
                .WithOne(f => f.Profissional)
                .HasForeignKey(f => f.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Favoritos)
                .WithOne(f => f.Cliente)
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.DenunciasEnviadas)
                .WithOne(d => d.Denunciante)
                .HasForeignKey(d => d.DenuncianteId)
                .OnDelete(DeleteBehavior.Restrict);

            // RELAÇÕES PARA SERVIÇOS PROFISSIONAIS E PORTFÓLIO
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ServicosProfissionais)
                .WithOne(sp => sp.Profissional)
                .HasForeignKey(sp => sp.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.PortfolioItems)
                .WithOne(pi => pi.Profissional)
                .HasForeignKey(pi => pi.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuração de ServiçoProfissional
            modelBuilder.Entity<ServicoProfissional>()
                .HasMany(sp => sp.ExemplosTrabalhos)
                .WithOne(pi => pi.ServicoProfissional)
                .HasForeignKey(pi => pi.ServicoProfissionalId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuração de chaves compostas
            modelBuilder.Entity<Favorito>()
                .HasKey(f => new { f.ClienteId, f.ProfissionalId });

            // Configuração de soft delete para mensagens
            modelBuilder.Entity<Mensagem>()
                .Property(m => m.DeletedForSender)
                .HasDefaultValue(false);

            modelBuilder.Entity<Mensagem>()
                .Property(m => m.DeletedForReceiver)
                .HasDefaultValue(false);

            // Configuração de estados para serviços
            modelBuilder.Entity<Servico>()
                .Property(s => s.Estado)
                .HasConversion<string>()
                .HasDefaultValue(EstadoServico.Pendente);

            // Configuração de estados para denúncias
            modelBuilder.Entity<Denuncia>()
                .Property(d => d.Estado)
                .HasConversion<string>()
                .HasDefaultValue(EstadoDenuncia.Pendente);

            modelBuilder.Entity<Denuncia>()
                .Property(d => d.Tipo)
                .HasConversion<string>();

            // Configuração de PortfolioItem
            modelBuilder.Entity<PortfolioItem>()
                .Property(p => p.Tipo)
                .HasConversion<string>();

            // Relação entre Avaliação e Serviço
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Servico)
                .WithOne(s => s.Avaliacao)
                .HasForeignKey<Avaliacao>(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação entre Denúncia e Serviço (opcional)
            modelBuilder.Entity<Denuncia>()
                .HasOne(d => d.Servico)
                .WithMany()
                .HasForeignKey(d => d.ServicoId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ============================================
            // INDEXES PARA MELHOR PERFORMANCE
            // ============================================

            modelBuilder.Entity<Servico>()
                .HasIndex(s => s.Estado);

            modelBuilder.Entity<Servico>()
                .HasIndex(s => s.ClienteId);

            modelBuilder.Entity<Servico>()
                .HasIndex(s => s.ProfissionalId);

            modelBuilder.Entity<Mensagem>()
                .HasIndex(m => m.DestinatarioId);

            modelBuilder.Entity<Mensagem>()
                .HasIndex(m => m.RemetenteId);

            modelBuilder.Entity<Mensagem>()
                .HasIndex(m => m.IsLida);

            modelBuilder.Entity<Avaliacao>()
                .HasIndex(a => a.AvaliadoId);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Categoria);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.IsActive);
        }

        // Método para teste rápido
        public async Task<Dictionary<string, int>> GetDatabaseStats()
        {
            return new Dictionary<string, int>
            {
                ["Users"] = await Users.CountAsync(),
                ["Services"] = await Servicos.CountAsync(),
                ["Messages"] = await Mensagens.CountAsync(),
                ["Ratings"] = await Avaliacoes.CountAsync(),
                ["ProfessionalServices"] = await ServicosProfissionais.CountAsync(),
                ["PortfolioItems"] = await PortfolioItems.CountAsync()
            };
        }
    }
}