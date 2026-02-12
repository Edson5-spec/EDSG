using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class Avaliacao {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ServicoId { get; set; }

        [Required]
        public string AvaliadorId { get; set; }

        [Required]
        public string AvaliadoId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5")]
        [Display(Name = "Nota")]
        public int Nota { get; set; }

        [StringLength(1000, ErrorMessage = "O comentário não pode ter mais de 1000 caracteres")]
        [Display(Name = "Comentário")]
        public string? Comentario { get; set; }

        [Display(Name = "Data da Avaliação")]
        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;

        [StringLength(1000, ErrorMessage = "A resposta não pode ter mais de 1000 caracteres")]
        [Display(Name = "Resposta do Profissional")]
        public string? Resposta { get; set; }

        [Display(Name = "Data da Resposta")]
        public DateTime? DataResposta { get; set; }

        // Navigation properties
        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }

        [ForeignKey("AvaliadorId")]
        public virtual ApplicationUser Avaliador { get; set; }

        [ForeignKey("AvaliadoId")]
        public virtual ApplicationUser Avaliado { get; set; }
    }
}