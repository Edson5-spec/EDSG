using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EDSG.Controllers {
    [Authorize]
    public class ServicosProfissionaisController : Controller {
        private readonly AppDbContext _context;

        public ServicosProfissionaisController(AppDbContext context) {
            _context = context;
        }

        // GET: ServicosProfissionais
        public async Task<IActionResult> Index() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servicos = await _context.ServicosProfissionais
                .Where(s => s.ProfissionalId == userId)
                .OrderByDescending(s => s.DataCriacao)
                .ToListAsync();

            return View(servicos);
        }

        // GET: ServicosProfissionais/Create
        public IActionResult Create() {
            return View(new ServicoProfissionalViewModel());
        }

        // POST: ServicosProfissionais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServicoProfissionalViewModel model) {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var servico = new ServicoProfissional {
                ProfissionalId = userId,
                Nome = model.Nome,
                Descricao = model.Descricao,
                Categoria = model.Categoria,
                Preco = model.Preco,
                TempoEstimado = model.TempoEstimado,
                IsAtivo = true,
                DataCriacao = DateTime.UtcNow
            };

            _context.ServicosProfissionais.Add(servico);
            await _context.SaveChangesAsync();

            // Adicionar exemplos de trabalhos
            if (model.ExemplosTrabalhos != null && model.ExemplosTrabalhos.Any()) {
                foreach (var exemplo in model.ExemplosTrabalhos) {
                    if (!string.IsNullOrEmpty(exemplo.Titulo)) {
                        var portfolioItem = new PortfolioItem {
                            ProfissionalId = userId,
                            ServicoProfissionalId = servico.Id,
                            Titulo = exemplo.Titulo,
                            Descricao = exemplo.Descricao,
                            ImagemUrl = exemplo.ImagemUrl,
                            LinkProjeto = exemplo.LinkProjeto,
                            Tipo = exemplo.Tipo,
                            DataProjeto = exemplo.DataProjeto,
                            Ordem = exemplo.Ordem,
                            IsAtivo = true,
                            DataCriacao = DateTime.UtcNow
                        };
                        _context.PortfolioItems.Add(portfolioItem);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Serviço criado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServicosProfissionais/Edit/5
        public async Task<IActionResult> Edit(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servico = await _context.ServicosProfissionais
                .Include(s => s.ExemplosTrabalhos)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == userId);

            if (servico == null) return NotFound();

            var model = new ServicoProfissionalViewModel {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                Categoria = servico.Categoria,
                Preco = servico.Preco,
                TempoEstimado = servico.TempoEstimado,
                ExemplosTrabalhos = servico.ExemplosTrabalhos.Select(e => new ExemploTrabalhoViewModel {
                    Titulo = e.Titulo,
                    Descricao = e.Descricao,
                    ImagemUrl = e.ImagemUrl,
                    LinkProjeto = e.LinkProjeto,
                    Tipo = e.Tipo,
                    DataProjeto = e.DataProjeto,
                    Ordem = e.Ordem
                }).ToList()
            };

            return View(model);
        }

        // POST: ServicosProfissionais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServicoProfissionalViewModel model) {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servico = await _context.ServicosProfissionais
                .Include(s => s.ExemplosTrabalhos)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == userId);

            if (servico == null) return NotFound();

            servico.Nome = model.Nome;
            servico.Descricao = model.Descricao;
            servico.Categoria = model.Categoria;
            servico.Preco = model.Preco;
            servico.TempoEstimado = model.TempoEstimado;
            servico.DataAtualizacao = DateTime.UtcNow;

            // Atualizar exemplos de trabalhos
            var exemplosAntigos = servico.ExemplosTrabalhos.ToList();
            _context.PortfolioItems.RemoveRange(exemplosAntigos);

            if (model.ExemplosTrabalhos != null) {
                foreach (var exemplo in model.ExemplosTrabalhos) {
                    if (!string.IsNullOrEmpty(exemplo.Titulo)) {
                        var portfolioItem = new PortfolioItem {
                            ProfissionalId = userId,
                            ServicoProfissionalId = id,
                            Titulo = exemplo.Titulo,
                            Descricao = exemplo.Descricao,
                            ImagemUrl = exemplo.ImagemUrl,
                            LinkProjeto = exemplo.LinkProjeto,
                            Tipo = exemplo.Tipo,
                            DataProjeto = exemplo.DataProjeto,
                            Ordem = exemplo.Ordem,
                            IsAtivo = true,
                            DataCriacao = DateTime.UtcNow
                        };
                        _context.PortfolioItems.Add(portfolioItem);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Serviço atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: ServicosProfissionais/Desativar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desativar(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servico = await _context.ServicosProfissionais
                .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == userId);

            if (servico == null) return NotFound();

            servico.IsAtivo = false;
            servico.DataAtualizacao = DateTime.UtcNow;
            _context.Update(servico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço desativado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: ServicosProfissionais/Ativar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ativar(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servico = await _context.ServicosProfissionais
                .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == userId);

            if (servico == null) return NotFound();

            servico.IsAtivo = true;
            servico.DataAtualizacao = DateTime.UtcNow;
            _context.Update(servico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço ativado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // POST: ServicosProfissionais/Remover/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var servico = await _context.ServicosProfissionais
                .Include(s => s.ExemplosTrabalhos)
                .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == userId);

            if (servico == null) return NotFound();

            // Remover exemplos antes
            if (servico.ExemplosTrabalhos.Any()) {
                _context.PortfolioItems.RemoveRange(servico.ExemplosTrabalhos);
            }

            _context.ServicosProfissionais.Remove(servico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço removido da base de dados!";
            return RedirectToAction(nameof(Index));
        }

        // GET: ServicosProfissionais/Details/5
        public async Task<IActionResult> Details(int id) {
            var servico = await _context.ServicosProfissionais
                .Include(s => s.Profissional)
                .Include(s => s.ExemplosTrabalhos)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null) return NotFound();

            return View(servico);
        }

        // ==========================
        // ViewModels Internos
        // ==========================
        public class ServicoProfissionalViewModel {
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
            [Range(0, 10000)]
            public decimal Preco { get; set; }

            [Display(Name = "Tempo Estimado")]
            [StringLength(100)]
            public string? TempoEstimado { get; set; }

            [Display(Name = "Exemplos de Trabalhos")]
            public List<ExemploTrabalhoViewModel> ExemplosTrabalhos { get; set; } = new();
        }

        public class ExemploTrabalhoViewModel {
            [Required(ErrorMessage = "O título é obrigatório")]
            [StringLength(200)]
            [Display(Name = "Título")]
            public string Titulo { get; set; }

            [StringLength(1000)]
            [Display(Name = "Descrição")]
            public string? Descricao { get; set; }

            [StringLength(500)]
            [Display(Name = "Imagem URL")]
            [Url(ErrorMessage = "Insira um URL válido")]
            public string? ImagemUrl { get; set; }

            [StringLength(500)]
            [Display(Name = "Link do Projeto")]
            [Url(ErrorMessage = "Insira um URL válido")]
            public string? LinkProjeto { get; set; }

            [Display(Name = "Tipo")]
            public TipoPortfolio Tipo { get; set; } = TipoPortfolio.Imagem;

            [Display(Name = "Data do Projeto")]
            public DateTime? DataProjeto { get; set; }

            [Display(Name = "Ordem")]
            [Range(0, 100)]
            public int Ordem { get; set; } = 0;
        }
    }
}
