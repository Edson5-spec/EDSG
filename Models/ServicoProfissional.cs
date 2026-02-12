using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class ServicoProfissional {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(200, ErrorMessage = "O nome não pode ter mais de 200 caracteres")]
        [Display(Name = "Nome do Serviço")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string? Categoria { get; set; }

        [Display(Name = "Preço (€)")]
        [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10000")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Display(Name = "Tempo Estimado")]
        [StringLength(100)]
        public string? TempoEstimado { get; set; }

        [Required]
        public string ProfissionalId { get; set; }

        [ForeignKey("ProfissionalId")]
        [Display(Name = "Profissional")]
        public ApplicationUser Profissional { get; set; }

        [Display(Name = "Ativo")]
        public bool IsAtivo { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Display(Name = "Data de Atualização")]
        public DateTime? DataAtualizacao { get; set; }

        // Navigation property
        [Display(Name = "Exemplos de Trabalho")]
        public virtual ICollection<PortfolioItem> ExemplosTrabalhos { get; set; } = new List<PortfolioItem>();

        // Campos calculados
        [NotMapped]
        [Display(Name = "Avaliação Média")]
        public double? AvaliacaoMedia { get; set; }

        [NotMapped]
        [Display(Name = "Total de Avaliações")]
        public int TotalAvaliacoes { get; set; }

        [NotMapped]
        [Display(Name = "Vezes Contratado")]
        public int TotalContratacoes { get; set; }
    }
}