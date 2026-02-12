using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public enum EstadoServico {
        Pendente,
        Aceite,
        Recusado,
        EmProgresso,
        Concluido,
        Cancelado
    }

    public class Servico {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClienteId { get; set; }

        [Required]
        public string ProfissionalId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string? Categoria { get; set; }

        [Display(Name = "Localização")]
        [StringLength(200)]
        public string? Localizacao { get; set; }

        [Display(Name = "Preço Acordado")]
        [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10000")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoAcordado { get; set; }

        [Display(Name = "Estado")]
        public EstadoServico Estado { get; set; } = EstadoServico.Pendente;

        [Display(Name = "Data de Pedido")]
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        [Display(Name = "Data de Aceitação")]
        public DateTime? DataAceitacao { get; set; }

        [Display(Name = "Data de Conclusão")]
        public DateTime? DataConclusao { get; set; }

        [Display(Name = "Nota do Cliente")]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5")]
        public int? NotaCliente { get; set; }

        [Display(Name = "Comentário do Cliente")]
        [StringLength(1000)]
        public string? ComentarioCliente { get; set; }

        [Display(Name = "Resposta do Profissional")]
        [StringLength(1000)]
        public string? RespostaProfissional { get; set; }

        // Navigation properties
        [ForeignKey("ClienteId")]
        public virtual ApplicationUser Cliente { get; set; }

        [ForeignKey("ProfissionalId")]
        public virtual ApplicationUser Profissional { get; set; }

        public virtual Avaliacao? Avaliacao { get; set; }
    }
}