using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class Mensagem {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RemetenteId { get; set; }

        [Required]
        public string DestinatarioId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "A mensagem não pode ter mais de 1000 caracteres")]
        [Display(Name = "Mensagem")]
        public string Texto { get; set; }

        [Display(Name = "Data de Envio")]
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

        [Display(Name = "Lida")]
        public bool IsLida { get; set; } = false;

        [Display(Name = "Apagada para Remetente")]
        public bool DeletedForSender { get; set; } = false;

        [Display(Name = "Apagada para Destinatário")]
        public bool DeletedForReceiver { get; set; } = false;

        // Navigation properties
        [ForeignKey("RemetenteId")]
        public virtual ApplicationUser Remetente { get; set; }

        [ForeignKey("DestinatarioId")]
        public virtual ApplicationUser Destinatario { get; set; }
    }
}