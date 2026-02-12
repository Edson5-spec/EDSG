using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class ApplicationUser : IdentityUser {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
        public string Nome { get; set; }

        [Display(Name = "Localização")]
        [StringLength(200, ErrorMessage = "A localização não pode ter mais de 200 caracteres")]
        public string? Localizacao { get; set; }

        [Display(Name = "Categoria Profissional")]
        [StringLength(100, ErrorMessage = "A categoria não pode ter mais de 100 caracteres")]
        public string? Categoria { get; set; }

        [Display(Name = "Especialidade")]
        [StringLength(200, ErrorMessage = "A especialidade não pode ter mais de 200 caracteres")]
        public string? Especialidade { get; set; }

        [Display(Name = "Preço Base (€/hora)")]
        [Range(0, 1000, ErrorMessage = "O preço deve estar entre 0 e 1000")]
        public decimal? PrecoBase { get; set; }

        [Display(Name = "Biografia")]
        [StringLength(2000, ErrorMessage = "A biografia não pode ter mais de 2000 caracteres")]
        public string? Bio { get; set; }

        [Display(Name = "Portfólio (PDF)")]
        public string? PortfolioFile { get; set; }

        [Display(Name = "Nome do Arquivo do Portfólio")]
        public string? PortfolioFileName { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        [Display(Name = "Administrador")]
        public bool IsAdmin { get; set; } = false;

        [Display(Name = "Premium")]
        public bool IsPremium { get; set; } = false;

        [Display(Name = "Conta Ativa")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Data de Registro")]
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;

        [Display(Name = "Última Atualização")]
        public DateTime? UltimaAtualizacao { get; set; }

        // Campos calculados (não armazenados na BD)
        [NotMapped]
        public double AvaliacaoMedia { get; set; }

        [NotMapped]
        public int TotalAvaliacoes { get; set; }

        [NotMapped]
        public int TotalServicosConcluidos { get; set; }

        [NotMapped]
        public int TotalFavoritos { get; set; }

        [NotMapped]
        public bool IsFavorito { get; set; }

        // Navigation properties
        public virtual ICollection<Servico> ServicosComoCliente { get; set; }
        public virtual ICollection<Servico> ServicosComoProfissional { get; set; }
        public virtual ICollection<ServicoProfissional> ServicosProfissionais { get; set; }
        public virtual ICollection<Avaliacao> AvaliacoesRecebidas { get; set; }
        public virtual ICollection<Avaliacao> AvaliacoesEnviadas { get; set; }
        public virtual ICollection<Mensagem> MensagensEnviadas { get; set; }
        public virtual ICollection<Mensagem> MensagensRecebidas { get; set; }
        public virtual ICollection<Favorito> FavoritadoPor { get; set; }
        public virtual ICollection<Favorito> Favoritos { get; set; }
        public virtual ICollection<Denuncia> DenunciasEnviadas { get; set; }
        public virtual ICollection<PortfolioItem> PortfolioItems { get; set; }

        public ApplicationUser() {
            ServicosComoCliente = new HashSet<Servico>();
            ServicosComoProfissional = new HashSet<Servico>();
            ServicosProfissionais = new HashSet<ServicoProfissional>();
            AvaliacoesRecebidas = new HashSet<Avaliacao>();
            AvaliacoesEnviadas = new HashSet<Avaliacao>();
            MensagensEnviadas = new HashSet<Mensagem>();
            MensagensRecebidas = new HashSet<Mensagem>();
            FavoritadoPor = new HashSet<Favorito>();
            Favoritos = new HashSet<Favorito>();
            DenunciasEnviadas = new HashSet<Denuncia>();
            PortfolioItems = new HashSet<PortfolioItem>();
        }
    }
}