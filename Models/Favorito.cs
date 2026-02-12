using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDSG.Models {
    public class Favorito {
        [Required]
        public string ClienteId { get; set; }

        [Required]
        public string ProfissionalId { get; set; }

        [Display(Name = "Data de Adição")]
        public DateTime DataAdicao { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ClienteId")]
        public virtual ApplicationUser Cliente { get; set; }

        [ForeignKey("ProfissionalId")]
        public virtual ApplicationUser Profissional { get; set; }
    }
}