using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public enum TipoDenuncia {
        ServicoInadequado,
        ComportamentoInadequado,
        ConteudoInadequado,
        Outro
    }

    public enum EstadoDenuncia {
        Pendente,
        EmAnalise,
        Resolvida,
        Rejeitada
    }

    public class Denuncia {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DenuncianteId { get; set; }

        [Required]
        public string DenunciadoId { get; set; }

        [Display(Name = "Tipo de Denúncia")]
        public TipoDenuncia Tipo { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Estado")]
        public EstadoDenuncia Estado { get; set; } = EstadoDenuncia.Pendente;

        [Display(Name = "Data da Denúncia")]
        public DateTime DataDenuncia { get; set; } = DateTime.UtcNow;

        [Display(Name = "Data de Resolução")]
        public DateTime? DataResolucao { get; set; }

        [StringLength(1000, ErrorMessage = "As notas não podem ter mais de 1000 caracteres")]
        [Display(Name = "Notas do Administrador")]
        public string? NotasAdmin { get; set; }

        public int? ServicoId { get; set; }

        // Navigation properties
        [ForeignKey("DenuncianteId")]
        public virtual ApplicationUser Denunciante { get; set; }

        [ForeignKey("DenunciadoId")]
        public virtual ApplicationUser Denunciado { get; set; }

        [ForeignKey("ServicoId")]
        public virtual Servico? Servico { get; set; }
    }
}