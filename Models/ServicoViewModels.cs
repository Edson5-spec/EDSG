using System.ComponentModel.DataAnnotations;

namespace EDSG.Models {
    public class NovoServicoViewModel {
        [Required]
        public string ProfissionalId { get; set; }

        [Display(Name = "Profissional")]
        public string ProfissionalNome { get; set; }

        [Display(Name = "Categoria")]
        public string ProfissionalCategoria { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres")]
        [Display(Name = "Título do Serviço")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; }

        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Categoria { get; set; }

        [Display(Name = "Localização")]
        [StringLength(200)]
        public string Localizacao { get; set; }

        [Display(Name = "Preço Acordado (€)")]
        [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10000")]
        public decimal Preco { get; set; }

        [Display(Name = "Preço Sugerido (€)")]
        public decimal PrecoSugerido { get; set; }
    }

    public class EditarServicoViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título não pode ter mais de 200 caracteres")]
        [Display(Name = "Título do Serviço")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres")]
        [Display(Name = "Descrição Detalhada")]
        public string Descricao { get; set; }

        [Display(Name = "Categoria")]
        [StringLength(100)]
        public string Categoria { get; set; }

        [Display(Name = "Localização")]
        [StringLength(200)]
        public string Localizacao { get; set; }

        [Display(Name = "Preço Acordado (€)")]
        [Range(0, 10000, ErrorMessage = "O preço deve estar entre 0 e 10000")]
        public decimal PrecoAcordado { get; set; }
    }

    public class AvaliarServicoViewModel {
        [Required]
        public int ServicoId { get; set; }

        [Display(Name = "Profissional")]
        public string ProfissionalNome { get; set; }

        [Required(ErrorMessage = "A nota é obrigatória")]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5")]
        [Display(Name = "Nota (1-5)")]
        public int Nota { get; set; }

        [StringLength(1000, ErrorMessage = "O comentário não pode ter mais de 1000 caracteres")]
        [Display(Name = "Comentário (opcional)")]
        public string? Comentario { get; set; }
    }
}