using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;

namespace EDSG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                TotalServicos = await _context.Servicos.CountAsync(),
                TotalProfissionais = await _context.Users.CountAsync(u => u.Categoria != null && u.IsActive),
                MelhoresAvaliados = await GetMelhoresAvaliados(),
                ServicosRecentes = await GetServicosRecentes()
            };

            return View(viewModel);
        }

        // GET: Home/Procurar - COM FILTRO CONVENCIONAL DE ESTRELAS
        public async Task<IActionResult> Procurar(string? categoria, string? localizacao, string? termo,
            int? precoMin, int? precoMax, string? ratingFilter, bool? premiumOnly, int pagina = 1)
        {
            const int profissionaisPorPagina = 10;

            Console.WriteLine($"=== FILTROS APLICADOS ===");
            Console.WriteLine($"Categoria: {categoria}");
            Console.WriteLine($"Localização: {localizacao}");
            Console.WriteLine($"Termo: {termo}");
            Console.WriteLine($"Preço Min: {precoMin}");
            Console.WriteLine($"Preço Max: {precoMax}");
            Console.WriteLine($"Rating Filter: {ratingFilter}");
            Console.WriteLine($"Premium Only: {premiumOnly}");
            Console.WriteLine($"Página: {pagina}");

            var query = _context.Users.AsQueryable();

            // Apenas profissionais ativos (têm categoria definida e estão ativos)
            query = query.Where(u => u.Categoria != null && u.IsActive);
            Console.WriteLine($"Total profissionais ativos: {await query.CountAsync()}");

            // Filtros básicos
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(u => u.Categoria.ToLower().Contains(categoria.ToLower()));
                Console.WriteLine($"Filtro categoria '{categoria}': {await query.CountAsync()} resultados");
            }

            if (!string.IsNullOrEmpty(localizacao))
            {
                query = query.Where(u => u.Localizacao != null &&
                    u.Localizacao.ToLower().Contains(localizacao.ToLower()));
                Console.WriteLine($"Filtro localização '{localizacao}': {await query.CountAsync()} resultados");
            }

            if (!string.IsNullOrEmpty(termo))
            {
                termo = termo.ToLower();
                query = query.Where(u =>
                    u.Nome.ToLower().Contains(termo) ||
                    (u.Especialidade != null && u.Especialidade.ToLower().Contains(termo)) ||
                    (u.Bio != null && u.Bio.ToLower().Contains(termo)));
                Console.WriteLine($"Filtro termo '{termo}': {await query.CountAsync()} resultados");
            }

            if (precoMin.HasValue && precoMin.Value > 0)
            {
                query = query.Where(u => u.PrecoBase >= precoMin.Value);
                Console.WriteLine($"Filtro preço mínimo {precoMin}: {await query.CountAsync()} resultados");
            }

            if (precoMax.HasValue && precoMax.Value > 0)
            {
                query = query.Where(u => u.PrecoBase <= precoMax.Value);
                Console.WriteLine($"Filtro preço máximo {precoMax}: {await query.CountAsync()} resultados");
            }

            if (premiumOnly == true)
            {
                query = query.Where(u => u.IsPremium);
                Console.WriteLine($"Filtro premium only: {await query.CountAsync()} resultados");
            }

            // Calcular total de resultados ANTES de filtrar por rating
            var totalAntesRating = await query.CountAsync();
            Console.WriteLine($"Total antes do filtro de rating: {totalAntesRating}");

            // FILTRO CONVENCIONAL DE ESTRELAS (como Amazon/Booking)
            if (!string.IsNullOrEmpty(ratingFilter))
            {
                Console.WriteLine($"Aplicando filtro de rating: '{ratingFilter}'");

                // Carregar todos os profissionais com suas avaliações
                var profissionaisComAvaliacoes = await query
                    .Include(u => u.AvaliacoesRecebidas)
                    .ToListAsync();

                List<string> profissionaisFiltrados = new List<string>();

                switch (ratingFilter)
                {
                    case "5":
                        // Exatamente 5 estrelas
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any() &&
                                Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 4.9)
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '5 estrelas': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "4":
                        // 4 estrelas ou mais (mas menos de 5)
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any())
                            .Where(u => {
                                double media = Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1);
                                return media >= 3.9 && media < 4.9;
                            })
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '4 estrelas': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "3":
                        // 3 estrelas ou mais (mas menos de 4)
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any())
                            .Where(u => {
                                double media = Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1);
                                return media >= 2.9 && media < 3.9;
                            })
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '3 estrelas': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "2":
                        // 2 estrelas ou mais (mas menos de 3)
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any())
                            .Where(u => {
                                double media = Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1);
                                return media >= 1.9 && media < 2.9;
                            })
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '2 estrelas': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "1":
                        // 1 estrela (ou seja, menos de 2)
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any())
                            .Where(u => {
                                double media = Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1);
                                return media < 1.9;
                            })
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '1 estrela': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "4plus":
                        // 4 estrelas ou mais
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any() &&
                                Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 3.9)
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '4+ estrelas': {profissionaisFiltrados.Count} resultados");
                        break;

                    case "3plus":
                        // 3 estrelas ou mais
                        profissionaisFiltrados = profissionaisComAvaliacoes
                            .Where(u => u.AvaliacoesRecebidas != null && u.AvaliacoesRecebidas.Any() &&
                                Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 2.9)
                            .Select(u => u.Id)
                            .ToList();
                        Console.WriteLine($"Filtro '3+ estrelas': {profissionaisFiltrados.Count} resultados");
                        break;
                }

                // Aplicar filtro no query
                if (profissionaisFiltrados.Any())
                {
                    query = query.Where(u => profissionaisFiltrados.Contains(u.Id));
                }
                else
                {
                    // Se não houver resultados, forçar query vazia
                    query = query.Where(u => false);
                }
            }

            // Calcular total de resultados
            var totalResultados = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalResultados / profissionaisPorPagina);

            // Ajustar página atual se necessário
            pagina = Math.Max(1, Math.Min(pagina, totalPaginas));

            Console.WriteLine($"Total resultados finais: {totalResultados}");
            Console.WriteLine($"Total páginas: {totalPaginas}");

            // Aplicar paginação - Carregar avaliações também
            var profissionais = await query
                .Include(u => u.AvaliacoesRecebidas)
                .OrderByDescending(u => u.IsPremium) // Premium primeiro
                .ThenByDescending(u => u.AvaliacoesRecebidas.Any() ?
                    u.AvaliacoesRecebidas.Average(a => a.Nota) : 0)
                .Skip((pagina - 1) * profissionaisPorPagina)
                .Take(profissionaisPorPagina)
                .ToListAsync();

            // Calcular avaliação média para cada profissional
            foreach (var profissional in profissionais)
            {
                if (profissional.AvaliacoesRecebidas != null && profissional.AvaliacoesRecebidas.Any())
                {
                    profissional.AvaliacaoMedia = Math.Round(
                        profissional.AvaliacoesRecebidas.Average(a => (double)a.Nota),
                        1
                    );
                    profissional.TotalAvaliacoes = profissional.AvaliacoesRecebidas.Count;
                }
                else
                {
                    profissional.AvaliacaoMedia = 0;
                    profissional.TotalAvaliacoes = 0;
                }

                Console.WriteLine($"Profissional: {profissional.Nome}, Rating: {profissional.AvaliacaoMedia}, Preço: {profissional.PrecoBase}");
            }

            // Passar dados para View
            ViewBag.Categoria = categoria;
            ViewBag.Localizacao = localizacao;
            ViewBag.Termo = termo;
            ViewBag.PrecoMin = precoMin;
            ViewBag.PrecoMax = precoMax;
            ViewBag.RatingFilter = ratingFilter;
            ViewBag.PremiumOnly = premiumOnly;
            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalResultados = totalResultados;
            ViewBag.ProfissionaisPorPagina = profissionaisPorPagina;

            // Obter categorias distintas para dropdown
            ViewBag.Categorias = await _context.Users
                .Where(u => u.Categoria != null && u.IsActive)
                .Select(u => u.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Calcular estatísticas para sidebar
            try
            {
                // 1. Avaliação média geral
                var avaliacoes = await _context.Avaliacoes
                    .Where(a => a.Avaliado.Categoria != null && a.Avaliado.IsActive)
                    .Select(a => (double)a.Nota)
                    .ToListAsync();

                ViewBag.AvaliacaoMediaGeral = avaliacoes.Any() ?
                    Math.Round(avaliacoes.Average(), 1) : 0;

                // 2. Preço médio
                var precos = await _context.Users
                    .Where(u => u.Categoria != null && u.IsActive && u.PrecoBase.HasValue)
                    .Select(u => (double)u.PrecoBase.Value)
                    .ToListAsync();

                ViewBag.PrecoMedio = precos.Any() ?
                    (decimal)Math.Round(precos.Average(), 2) : 0;

                // 3. Total premium
                ViewBag.TotalPremium = await _context.Users
                    .CountAsync(u => u.Categoria != null && u.IsActive && u.IsPremium);

                // 4. Distribuição por rating (para mostrar nos filtros)
                var profissionaisAtivos = await _context.Users
                    .Where(u => u.Categoria != null && u.IsActive)
                    .Include(u => u.AvaliacoesRecebidas)
                    .ToListAsync();

                // Calcular contagem por faixa de rating
                ViewBag.RatingDistribution = new Dictionary<string, int>
                {
                    ["5"] = profissionaisAtivos.Count(u => u.AvaliacoesRecebidas.Any() &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 4.9),
                    ["4"] = profissionaisAtivos.Count(u => u.AvaliacoesRecebidas.Any() &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 3.9 &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) < 4.9),
                    ["3"] = profissionaisAtivos.Count(u => u.AvaliacoesRecebidas.Any() &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 2.9 &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) < 3.9),
                    ["2"] = profissionaisAtivos.Count(u => u.AvaliacoesRecebidas.Any() &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) >= 1.9 &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) < 2.9),
                    ["1"] = profissionaisAtivos.Count(u => u.AvaliacoesRecebidas.Any() &&
                        Math.Round(u.AvaliacoesRecebidas.Average(a => a.Nota), 1) < 1.9),
                    ["0"] = profissionaisAtivos.Count(u => !u.AvaliacoesRecebidas.Any())
                };

                Console.WriteLine($"Distribuição de ratings: 5={ViewBag.RatingDistribution["5"]}, 4={ViewBag.RatingDistribution["4"]}, 3={ViewBag.RatingDistribution["3"]}, 2={ViewBag.RatingDistribution["2"]}, 1={ViewBag.RatingDistribution["1"]}, Sem={ViewBag.RatingDistribution["0"]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao calcular estatísticas: {ex.Message}");
                ViewBag.AvaliacaoMediaGeral = 0;
                ViewBag.PrecoMedio = 0;
                ViewBag.TotalPremium = 0;
                ViewBag.RatingDistribution = new Dictionary<string, int>
                {
                    ["5"] = 0,
                    ["4"] = 0,
                    ["3"] = 0,
                    ["2"] = 0,
                    ["1"] = 0,
                    ["0"] = 0
                };
            }

            Console.WriteLine($"=== FILTROS FINALIZADOS ===");
            Console.WriteLine($"Enviando {profissionais.Count} profissionais para a view");

            return View(profissionais);
        }

        // GET: Home/DetalhesProfissional/{id}
        public async Task<IActionResult> DetalhesProfissional(string id)
        {
            var profissional = await _context.Users
                .Include(u => u.AvaliacoesRecebidas)
                    .ThenInclude(a => a.Avaliador)
                .FirstOrDefaultAsync(u => u.Id == id && u.Categoria != null && u.IsActive);

            if (profissional == null)
            {
                return NotFound();
            }

            // Calcular estatísticas
            var servicosConcluidos = await _context.Servicos
                .CountAsync(s => s.ProfissionalId == id && s.Estado == EstadoServico.Concluido);

            var totalAvaliacoes = profissional.AvaliacoesRecebidas?.Count ?? 0;
            var avaliacaoMedia = totalAvaliacoes > 0
                ? Math.Round(profissional.AvaliacoesRecebidas.Average(a => (double)a.Nota), 1)
                : 0;

            // Verificar se está disponível (simulação)
            ViewBag.DisponivelAgora = new Random().Next(0, 10) > 3;

            var viewModel = new DetalhesProfissionalViewModel
            {
                Profissional = profissional,
                ServicosConcluidos = servicosConcluidos,
                TotalAvaliacoes = totalAvaliacoes,
                AvaliacaoMedia = avaliacaoMedia
            };

            return View(viewModel);
        }

        // GET: Home/Sobre
        public IActionResult Sobre()
        {
            return View();
        }

        // GET: Home/Contactos
        public IActionResult Contactos()
        {
            return View();
        }

        // POST: Home/Contactos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contactos(ContactoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Aqui normalmente enviaria um email
                TempData["SuccessMessage"] = "A sua mensagem foi enviada com sucesso. Entraremos em contacto brevemente.";
                return RedirectToAction(nameof(Contactos));
            }

            return View(model);
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Métodos auxiliares privados
        private async Task<List<ApplicationUser>> GetMelhoresAvaliados()
        {
            var profissionais = await _context.Users
                .Where(u => u.Categoria != null && u.IsActive)
                .Include(u => u.AvaliacoesRecebidas)
                .Take(6)
                .ToListAsync();

            // Calcular avaliação média localmente
            foreach (var p in profissionais)
            {
                p.AvaliacaoMedia = p.AvaliacoesRecebidas != null && p.AvaliacoesRecebidas.Any()
                    ? Math.Round(p.AvaliacoesRecebidas.Average(a => (double)a.Nota), 1)
                    : 0;
            }

            return profissionais
                .OrderByDescending(p => p.AvaliacaoMedia)
                .ThenByDescending(p => p.AvaliacoesRecebidas != null ? p.AvaliacoesRecebidas.Count : 0)
                .ToList();
        }

        private async Task<List<Servico>> GetServicosRecentes()
        {
            return await _context.Servicos
                .Include(s => s.Cliente)
                .Include(s => s.Profissional)
                .Where(s => s.Estado == EstadoServico.Concluido)
                .OrderByDescending(s => s.DataConclusao)
                .Take(5)
                .ToListAsync();
        }
    }

    // ViewModels para HomeController
    public class HomeViewModel
    {
        public int TotalServicos { get; set; }
        public int TotalProfissionais { get; set; }
        public List<ApplicationUser> MelhoresAvaliados { get; set; }
        public List<Servico> ServicosRecentes { get; set; }
    }

    public class DetalhesProfissionalViewModel
    {
        public ApplicationUser Profissional { get; set; }
        public int ServicosConcluidos { get; set; }
        public int TotalAvaliacoes { get; set; }
        public double AvaliacaoMedia { get; set; }
    }

    public class ContactoViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O assunto é obrigatório")]
        [StringLength(200, ErrorMessage = "O assunto não pode ter mais de 200 caracteres")]
        [Display(Name = "Assunto")]
        public string Assunto { get; set; }

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(5000, ErrorMessage = "A mensagem não pode ter mais de 5000 caracteres")]
        [Display(Name = "Mensagem")]
        public string Mensagem { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}