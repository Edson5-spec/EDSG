using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class PortfolioItem {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [StringLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres")]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [StringLength(500)]
        [Display(Name = "URL da Imagem")]
        [Url(ErrorMessage = "Insira um URL válido")]
        public string? ImagemUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Link do Projeto")]
        [Url(ErrorMessage = "Insira um URL válido")]
        public string? LinkProjeto { get; set; }

        [Display(Name = "Tipo")]
        public TipoPortfolio Tipo { get; set; } = TipoPortfolio.Imagem;

        [Display(Name = "Data do Projeto")]
        [DataType(DataType.Date)]
        public DateTime? DataProjeto { get; set; }

        [Display(Name = "Ordem de Exibição")]
        [Range(0, 100, ErrorMessage = "A ordem deve estar entre 0 e 100")]
        public int Ordem { get; set; } = 0;

        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        [Display(Name = "Destacado")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Required]
        public string ProfissionalId { get; set; }

        [ForeignKey("ProfissionalId")]
        [Display(Name = "Profissional")]
        public ApplicationUser Profissional { get; set; }

        [Required]
        public int ServicoProfissionalId { get; set; }

        [ForeignKey("ServicoProfissionalId")]
        [Display(Name = "Serviço Profissional")]
        public ServicoProfissional ServicoProfissional { get; set; }
    }

    public enum TipoPortfolio {
        [Display(Name = "Imagem")]
        Imagem,

        [Display(Name = "Vídeo")]
        Video,

        [Display(Name = "Documento")]
        Documento,

        [Display(Name = "Link")]
        Link,

        [Display(Name = "PDF")]
        Pdf,

        [Display(Name = "Apresentação")]
        Apresentacao
    }
}