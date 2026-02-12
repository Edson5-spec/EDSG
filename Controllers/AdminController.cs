using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EDSG.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager) {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Index
        public async Task<IActionResult> Index() {
            var stats = new AdminStatsViewModel {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProfessionals = await _context.Users.CountAsync(u => u.Categoria != null),
                TotalServices = await _context.Servicos.CountAsync(),
                PendingServices = await _context.Servicos.CountAsync(s => s.Estado == EstadoServico.Pendente),
                ActiveServices = await _context.Servicos.CountAsync(s => s.Estado == EstadoServico.Aceite || s.Estado == EstadoServico.EmProgresso),
                CompletedServices = await _context.Servicos.CountAsync(s => s.Estado == EstadoServico.Concluido),
                PendingReports = await _context.Denuncias.CountAsync(d => d.Estado == EstadoDenuncia.Pendente),

                // CORRIGIDO: Usar Email para ordenação em vez de DataRegistro
                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.Email) // Alternativa: usar Email ou UserName
                    .Take(10)
                    .ToListAsync(),

                RecentServices = await _context.Servicos
                    .Include(s => s.Cliente)
                    .Include(s => s.Profissional)
                    .OrderByDescending(s => s.DataPedido)
                    .Take(10)
                    .ToListAsync()
            };

            return View(stats);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string? search, string? role, bool? active) {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(u =>
                    u.Nome.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Localizacao != null && u.Localizacao.Contains(search));
            }

            if (role == "professional") {
                query = query.Where(u => u.Categoria != null);
            } else if (role == "client") {
                query = query.Where(u => u.Categoria == null);
            }

            if (active.HasValue) {
                query = query.Where(u => u.IsActive == active.Value);
            }

            var users = await query.ToListAsync();

            // Criar ViewModel para mostrar se o usuário é Admin
            var userViewModels = new List<UserAdminViewModel>();
            foreach (var user in users) {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                userViewModels.Add(new UserAdminViewModel {
                    User = user,
                    IsAdmin = isAdmin,
                    // Pode calcular idade da conta se quiser (opcional)
                    DiasDesdeRegistro = 0 // Será calculado quando DataRegistro existir
                });
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Active = active;

            return View(userViewModels);
        }

        // POST: Admin/ToggleUserStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) {
                TempData["SuccessMessage"] = $"Estado do utilizador {user.Email} alterado para {(user.IsActive ? "Ativo" : "Inativo")}";
            } else {
                TempData["ErrorMessage"] = "Erro ao alterar o estado do utilizador";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/ToggleAdminRole/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin")) {
                // Remover role Admin
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                user.IsAdmin = false;
                TempData["SuccessMessage"] = $"Privilégios de admin removidos de {user.Email}";
            } else {
                // Adicionar role Admin
                await _userManager.AddToRoleAsync(user, "Admin");
                user.IsAdmin = true;
                TempData["SuccessMessage"] = $"Privilégios de admin concedidos a {user.Email}";
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Users));
        }

        // GET: Admin/ServicosProfissionais
        public async Task<IActionResult> ServicosProfissionais(string? search, string? categoria, bool? ativo) {
            var query = _context.ServicosProfissionais
                .Include(sp => sp.Profissional)
                .Include(sp => sp.ExemplosTrabalhos)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(sp =>
                    sp.Nome.Contains(search) ||
                    sp.Descricao.Contains(search) ||
                    sp.Profissional.Nome.Contains(search) ||
                    sp.Profissional.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(categoria)) {
                query = query.Where(sp => sp.Categoria == categoria);
            }

            if (ativo.HasValue) {
                query = query.Where(sp => sp.IsAtivo == ativo.Value);
            }

            var servicos = await query
                .OrderByDescending(sp => sp.DataCriacao)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Categoria = categoria;
            ViewBag.Ativo = ativo;

            return View(servicos);
        }

        // POST: Admin/ToggleServicoProfissionalStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleServicoProfissionalStatus(int id) {
            var servico = await _context.ServicosProfissionais.FindAsync(id);
            if (servico == null) {
                return NotFound();
            }

            servico.IsAtivo = !servico.IsAtivo;
            servico.DataAtualizacao = DateTime.UtcNow;

            _context.ServicosProfissionais.Update(servico);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Serviço profissional {(servico.IsAtivo ? "ativado" : "desativado")} com sucesso";
            return RedirectToAction(nameof(ServicosProfissionais));
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Reports(EstadoDenuncia? status) {
            var query = _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.Denunciado)
                .Include(d => d.Servico)
                .AsQueryable();

            if (status.HasValue) {
                query = query.Where(d => d.Estado == status.Value);
            }

            var reports = await query
                .OrderByDescending(d => d.DataDenuncia)
                .ToListAsync();

            ViewBag.Status = status;
            return View(reports);
        }

        // GET: Admin/ReportDetails/{id}
        public async Task<IActionResult> ReportDetails(int id) {
            var report = await _context.Denuncias
                .Include(d => d.Denunciante)
                .Include(d => d.Denunciado)
                .Include(d => d.Servico)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (report == null) {
                return NotFound();
            }

            return View(report);
        }

        // POST: Admin/UpdateReportStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReportStatus(int id, EstadoDenuncia status, string? adminNotes) {
            var report = await _context.Denuncias.FindAsync(id);
            if (report == null) {
                return NotFound();
            }

            report.Estado = status;
            report.NotasAdmin = adminNotes;
            report.DataResolucao = DateTime.UtcNow;

            _context.Denuncias.Update(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estado da denúncia atualizado com sucesso";
            return RedirectToAction(nameof(ReportDetails), new { id });
        }

        // GET: Admin/Services
        public async Task<IActionResult> Services(EstadoServico? status, string? search) {
            var query = _context.Servicos
                .Include(s => s.Cliente)
                .Include(s => s.Profissional)
                .AsQueryable();

            if (status.HasValue) {
                query = query.Where(s => s.Estado == status.Value);
            }

            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(s =>
                    s.Titulo.Contains(search) ||
                    s.Descricao.Contains(search) ||
                    s.Cliente.Nome.Contains(search) ||
                    s.Profissional.Nome.Contains(search));
            }

            var services = await query
                .OrderByDescending(s => s.DataPedido)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Search = search;
            return View(services);
        }

        // POST: Admin/DeleteService/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id) {
            var service = await _context.Servicos.FindAsync(id);
            if (service == null) {
                return NotFound();
            }

            // Delete related evaluations
            var evaluations = await _context.Avaliacoes
                .Where(a => a.ServicoId == id)
                .ToListAsync();

            _context.Avaliacoes.RemoveRange(evaluations);
            _context.Servicos.Remove(service);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço eliminado com sucesso";
            return RedirectToAction(nameof(Services));
        }

        // GET: Admin/SystemStats
        public async Task<IActionResult> SystemStats() {
            var stats = new SystemStatsViewModel {
                // User Statistics
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                PremiumUsers = await _context.Users.CountAsync(u => u.IsPremium),
                Professionals = await _context.Users.CountAsync(u => u.Categoria != null),

                // Service Statistics
                TotalServices = await _context.Servicos.CountAsync(),
                ServicesThisMonth = await _context.Servicos
                    .CountAsync(s => s.DataPedido.Month == DateTime.UtcNow.Month &&
                                    s.DataPedido.Year == DateTime.UtcNow.Year),
                AverageServicePrice = await _context.Servicos
                    .Where(s => s.Estado == EstadoServico.Concluido)
                    .AverageAsync(s => (decimal?)s.PrecoAcordado) ?? 0,

                // Financial Statistics
                TotalRevenue = await _context.Servicos
                    .Where(s => s.Estado == EstadoServico.Concluido)
                    .SumAsync(s => (decimal?)s.PrecoAcordado) ?? 0,
                RevenueThisMonth = await _context.Servicos
                    .Where(s => s.Estado == EstadoServico.Concluido &&
                               s.DataConclusao.HasValue &&
                               s.DataConclusao.Value.Month == DateTime.UtcNow.Month &&
                               s.DataConclusao.Value.Year == DateTime.UtcNow.Year)
                    .SumAsync(s => (decimal?)s.PrecoAcordado) ?? 0,

                // Rating Statistics
                AverageRating = await _context.Avaliacoes
                    .AverageAsync(a => (double?)a.Nota) ?? 0,
                TotalRatings = await _context.Avaliacoes.CountAsync(),

                // Recent Growth - CORRIGIDO: Valor temporário
                NewUsersThisMonth = 0, // Será calculado quando DataRegistro existir

                // Category Distribution
                CategoryStats = await _context.Users
                    .Where(u => u.Categoria != null)
                    .GroupBy(u => u.Categoria)
                    .Select(g => new CategoryStat {
                        Category = g.Key ?? "Sem Categoria",
                        Count = g.Count(),
                        AveragePrice = g.Average(u => u.PrecoBase ?? 0)
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(10)
                    .ToListAsync(),

                // Serviços Profissionais Stats
                TotalServicosProfissionais = await _context.ServicosProfissionais.CountAsync(),
                ServicosProfissionaisAtivos = await _context.ServicosProfissionais.CountAsync(sp => sp.IsAtivo),
                ServicosProfissionaisPorCategoria = await _context.ServicosProfissionais
                    .GroupBy(sp => sp.Categoria ?? "Sem Categoria")
                    .Select(g => new CategoryStat {
                        Category = g.Key,
                        Count = g.Count(),
                        AveragePrice = g.Average(sp => sp.Preco)
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(10)
                    .ToListAsync(),

                // Portfolio Items Stats
                TotalPortfolioItems = await _context.PortfolioItems.CountAsync(),
                ActivePortfolioItems = await _context.PortfolioItems.CountAsync(pi => pi.IsAtivo)
            };

            return View(stats);
        }

        // GET: Admin/PortfolioItems
        public async Task<IActionResult> PortfolioItems(int? servicoProfissionalId, string? search) {
            var query = _context.PortfolioItems
                .Include(pi => pi.Profissional)
                .Include(pi => pi.ServicoProfissional)
                .AsQueryable();

            if (servicoProfissionalId.HasValue) {
                query = query.Where(pi => pi.ServicoProfissionalId == servicoProfissionalId.Value);
            }

            if (!string.IsNullOrEmpty(search)) {
                query = query.Where(pi =>
                    pi.Titulo.Contains(search) ||
                    pi.Descricao.Contains(search) ||
                    pi.Profissional.Nome.Contains(search) ||
                    pi.ServicoProfissional.Nome.Contains(search));
            }

            var portfolioItems = await query
                .OrderByDescending(pi => pi.DataCriacao)
                .ToListAsync();

            ViewBag.ServicoProfissionalId = servicoProfissionalId;
            ViewBag.Search = search;

            return View(portfolioItems);
        }

        // POST: Admin/TogglePortfolioItemStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePortfolioItemStatus(int id) {
            var portfolioItem = await _context.PortfolioItems.FindAsync(id);
            if (portfolioItem == null) {
                return NotFound();
            }

            portfolioItem.IsAtivo = !portfolioItem.IsAtivo;

            _context.PortfolioItems.Update(portfolioItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Item de portfólio {(portfolioItem.IsAtivo ? "ativado" : "desativado")} com sucesso";
            return RedirectToAction(nameof(PortfolioItems));
        }

        // GET: Admin/ViewUserProfile/{id}
        public async Task<IActionResult> ViewUserProfile(string id) {
            var user = await _context.Users
                .Include(u => u.ServicosComoCliente)
                .Include(u => u.ServicosComoProfissional)
                .Include(u => u.ServicosProfissionais)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) {
                return NotFound();
            }

            // Calcular estatísticas do usuário
            ViewBag.TotalServicesAsClient = user.ServicosComoCliente?.Count ?? 0;
            ViewBag.TotalServicesAsProfessional = user.ServicosComoProfissional?.Count ?? 0;
            ViewBag.TotalProfessionalServices = user.ServicosProfissionais?.Count ?? 0;
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            return View(user);
        }

        // POST: Admin/SendMessageToUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessageToUser(string id, string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                TempData["ErrorMessage"] = "A mensagem não pode estar vazia";
                return RedirectToAction(nameof(ViewUserProfile), new { id });
            }

            var adminUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(id);

            if (targetUser == null || adminUser == null) {
                return NotFound();
            }

            // CORREÇÃO AQUI: Usar os nomes corretos das propriedades do modelo Mensagem
            var adminMessage = new Mensagem {
                RemetenteId = adminUser.Id,
                DestinatarioId = targetUser.Id,
                Texto = $"[MENSAGEM ADMINISTRATIVA]\n\n{message}\n\n---\nEsta é uma mensagem enviada pela administração do EDSG.", // CORRIGIDO: Conteudo → Texto
                DataEnvio = DateTime.UtcNow,
                IsLida = false // CORRIGIDO: Lida → IsLida
            };

            _context.Mensagens.Add(adminMessage);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Mensagem enviada para {targetUser.Email}";
            return RedirectToAction(nameof(ViewUserProfile), new { id });
        }
    } // FIM DA CLASSE AdminController

    // ViewModels for AdminController
    public class AdminStatsViewModel {
        public int TotalUsers { get; set; }
        public int TotalProfessionals { get; set; }
        public int TotalServices { get; set; }
        public int PendingServices { get; set; }
        public int ActiveServices { get; set; }
        public int CompletedServices { get; set; }
        public int PendingReports { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; }
        public List<Servico> RecentServices { get; set; }
    }

    public class SystemStatsViewModel {
        // User Statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PremiumUsers { get; set; }
        public int Professionals { get; set; }

        // Service Statistics
        public int TotalServices { get; set; }
        public int ServicesThisMonth { get; set; }
        public decimal AverageServicePrice { get; set; }

        // Financial Statistics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Rating Statistics
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }

        // Recent Growth
        public int NewUsersThisMonth { get; set; }

        // Category Distribution
        public List<CategoryStat> CategoryStats { get; set; }

        // Serviços Profissionais Stats
        public int TotalServicosProfissionais { get; set; }
        public int ServicosProfissionaisAtivos { get; set; }
        public List<CategoryStat> ServicosProfissionaisPorCategoria { get; set; }

        // Portfolio Items Stats
        public int TotalPortfolioItems { get; set; }
        public int ActivePortfolioItems { get; set; }
    }

    public class CategoryStat {
        public string Category { get; set; }
        public int Count { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class UserAdminViewModel {
        public ApplicationUser User { get; set; }
        public bool IsAdmin { get; set; }
        public int DiasDesdeRegistro { get; set; } // Para usar quando DataRegistro existir
    }
}