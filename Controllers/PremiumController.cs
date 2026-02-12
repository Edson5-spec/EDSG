using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EDSG.Controllers {
    [Authorize]
    public class PremiumController : Controller {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PremiumController> _logger;

        public PremiumController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<PremiumController> logger) {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Premium/Index
        public async Task<IActionResult> Index() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var viewModel = new PremiumViewModel {
                IsPremium = user?.IsPremium ?? false,
                PremiumSince = await GetPremiumSinceDate(userId),
                PremiumBenefits = GetPremiumBenefits(),
                Statistics = await GetPremiumStatistics(userId)
            };

            return View(viewModel);
        }

        // GET: Premium/Benefits
        public IActionResult Benefits() {
            var benefits = new PremiumBenefitsViewModel {
                Benefits = GetPremiumBenefits(),
                Testimonials = GetTestimonials(),
                PricingPlans = GetPricingPlans()
            };

            return View(benefits);
        }

        // GET: Premium/Upgrade
        public async Task<IActionResult> Upgrade(string? plan = "monthly") {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.IsPremium == true) {
                TempData["InfoMessage"] = "Já é membro Premium!";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UpgradeViewModel {
                SelectedPlan = plan ?? "monthly",
                Plans = GetPricingPlans(),
                UserEmail = user?.Email
            };

            return View(viewModel);
        }

        // POST: Premium/ProcessUpgrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessUpgrade(UpgradeViewModel model) {
            if (!ModelState.IsValid) {
                model.Plans = GetPricingPlans();
                return View("Upgrade", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) {
                return NotFound();
            }

            // Em produção, aqui integraria com um gateway de pagamento
            // Por enquanto, apenas marcamos como premium
            user.IsPremium = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) {
                // Registrar a transação (simulado)
                var transaction = new PremiumTransaction {
                    UserId = userId,
                    PlanType = model.SelectedPlan,
                    Amount = GetPlanPrice(model.SelectedPlan),
                    TransactionDate = DateTime.UtcNow,
                    Status = "completed",
                    PaymentMethod = model.PaymentMethod
                };

                // Em produção, salvaríamos no banco de dados
                // _context.PremiumTransactions.Add(transaction);
                // await _context.SaveChangesAsync();

                _logger.LogInformation("Utilizador {UserId} atualizado para Premium", userId);

                TempData["SuccessMessage"] = "Parabéns! Agora é membro Premium da EDSG!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.Plans = GetPricingPlans();
            return View("Upgrade", model);
        }

        // GET: Premium/Manage
        public async Task<IActionResult> Manage() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.IsPremium != true) {
                TempData["ErrorMessage"] = "Não é membro Premium.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ManageSubscriptionViewModel {
                CurrentPlan = "premium",
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                AutoRenew = true,
                PaymentMethod = "Cartão terminado em 4242"
            };

            return View(viewModel);
        }

        // POST: Premium/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) {
                return NotFound();
            }

            user.IsPremium = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) {
                TempData["SuccessMessage"] = "Assinatura Premium cancelada. Obrigado por ter sido membro!";
                _logger.LogInformation("Utilizador {UserId} cancelou Premium", userId);
            } else {
                TempData["ErrorMessage"] = "Erro ao cancelar a assinatura.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Premium/Stats (apenas para testes)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Stats() {
            var stats = new PremiumStatsViewModel {
                TotalPremiumUsers = await _context.Users.CountAsync(u => u.IsPremium),
                PremiumUsersThisMonth = await _context.Users
                    .CountAsync(u => u.IsPremium && u.Id.Contains("@")), // Simulado
                RevenueThisMonth = await CalculateMonthlyRevenue(),
                ConversionRate = await CalculateConversionRate(),
                TopCategories = await GetTopPremiumCategories()
            };

            return View(stats);
        }

        // Métodos auxiliares privados
        private async Task<DateTime?> GetPremiumSinceDate(string userId) {
            // Em produção, buscar da tabela de transações
            // Por enquanto, simulado
            return await Task.FromResult<DateTime?>(DateTime.UtcNow.AddMonths(-3));
        }

        private List<PremiumBenefit> GetPremiumBenefits() {
            return new List<PremiumBenefit>
            {
                new PremiumBenefit
                {
                    Title = "Destaque nos Resultados",
                    Description = "Seu perfil aparece no topo das pesquisas",
                    Icon = "bi-award",
                    Color = "primary"
                },
                new PremiumBenefit
                {
                    Title = "Verificação de Perfil",
                    Description = "Selo de verificação para maior credibilidade",
                    Icon = "bi-patch-check",
                    Color = "success"
                },
                new PremiumBenefit
                {
                    Title = "Estatísticas Avançadas",
                    Description = "Acesso a análises detalhadas do seu desempenho",
                    Icon = "bi-graph-up",
                    Color = "info"
                },
                new PremiumBenefit
                {
                    Title = "Suporte Prioritário",
                    Description = "Atendimento exclusivo e resposta rápida",
                    Icon = "bi-headset",
                    Color = "warning"
                },
                new PremiumBenefit
                {
                    Title = "Sem Anúncios",
                    Description = "Experiência limpa sem distrações",
                    Icon = "bi-ad",
                    Color = "danger"
                },
                new PremiumBenefit
                {
                    Title = "Ferramentas Avançadas",
                    Description = "Recursos exclusivos para gestão de serviços",
                    Icon = "bi-tools",
                    Color = "secondary"
                }
            };
        }

        private List<Testimonial> GetTestimonials() {
            return new List<Testimonial>
            {
                new Testimonial
                {
                    Name = "Carlos Silva",
                    Profession = "Designer Gráfico",
                    Comment = "Com o Premium, tripliquei meus clientes em 2 meses!",
                    Rating = 5,
                    AvatarColor = "primary"
                },
                new Testimonial
                {
                    Name = "Ana Rodrigues",
                    Profession = "Programadora",
                    Comment = "O destaque nos resultados fez toda a diferença para meu negócio.",
                    Rating = 5,
                    AvatarColor = "success"
                },
                new Testimonial
                {
                    Name = "Miguel Santos",
                    Profession = "Consultor",
                    Comment = "As ferramentas avançadas economizam horas do meu trabalho semanal.",
                    Rating = 4,
                    AvatarColor = "warning"
                }
            };
        }

        private List<PricingPlan> GetPricingPlans() {
            return new List<PricingPlan>
            {
                new PricingPlan
                {
                    Name = "Mensal",
                    Id = "monthly",
                    Price = 9.99m,
                    Period = "mês",
                    Description = "Ideal para testar",
                    Features = new List<string>
                    {
                        "Todos os benefícios Premium",
                        "Faturação mensal",
                        "Cancelamento a qualquer momento",
                        "Sem compromisso de permanência"
                    },
                    IsPopular = false
                },
                new PricingPlan
                {
                    Name = "Anual",
                    Id = "yearly",
                    Price = 99.99m,
                    Period = "ano",
                    Description = "Mais econômico",
                    Features = new List<string>
                    {
                        "Todos os benefícios Premium",
                        "2 meses gratuitos",
                        "Economia de 20%",
                        "Prioridade no suporte"
                    },
                    IsPopular = true
                },
                new PricingPlan
                {
                    Name = "Profissional",
                    Id = "professional",
                    Price = 24.99m,
                    Period = "mês",
                    Description = "Para profissionais estabelecidos",
                    Features = new List<string>
                    {
                        "Todos os benefícios Premium",
                        "Perfil verificado em destaque",
                        "Estatísticas avançadas",
                        "Suporte 24/7",
                        "API de integração"
                    },
                    IsPopular = false
                }
            };
        }

        private decimal GetPlanPrice(string planId) {
            return planId switch {
                "monthly" => 9.99m,
                "yearly" => 99.99m,
                "professional" => 24.99m,
                _ => 9.99m
            };
        }

        private async Task<PremiumStatistics> GetPremiumStatistics(string userId) {
            var services = await _context.Servicos
                .Where(s => s.ProfissionalId == userId)
                .ToListAsync();

            var totalServices = services.Count;
            var completedServices = services.Count(s => s.Estado == EstadoServico.Concluido);
            var premiumSince = await GetPremiumSinceDate(userId);
            var daysAsPremium = premiumSince.HasValue ?
                (DateTime.UtcNow - premiumSince.Value).Days : 0;

            return new PremiumStatistics {
                TotalServices = totalServices,
                CompletedServices = completedServices,
                CompletionRate = totalServices > 0 ? (completedServices * 100 / totalServices) : 0,
                DaysAsPremium = daysAsPremium,
                EstimatedEarnings = services.Where(s => s.Estado == EstadoServico.Concluido)
                    .Sum(s => s.PrecoAcordado)
            };
        }

        private async Task<decimal> CalculateMonthlyRevenue() {
            // Simulado - em produção, calcular das transações
            var premiumCount = await _context.Users.CountAsync(u => u.IsPremium);
            return premiumCount * 9.99m;
        }

        private async Task<double> CalculateConversionRate() {
            var totalUsers = await _context.Users.CountAsync();
            var premiumUsers = await _context.Users.CountAsync(u => u.IsPremium);

            return totalUsers > 0 ? (premiumUsers * 100.0 / totalUsers) : 0;
        }

        private async Task<List<CategoryPremiumStat>> GetTopPremiumCategories() {
            return await _context.Users
                .Where(u => u.IsPremium && u.Categoria != null)
                .GroupBy(u => u.Categoria)
                .Select(g => new CategoryPremiumStat {
                    Category = g.Key ?? "Outros",
                    Count = g.Count(),
                    Percentage = 0 // Calculado no front-end
                })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToListAsync();
        }
    }

    // ViewModels para PremiumController
    public class PremiumViewModel {
        public bool IsPremium { get; set; }
        public DateTime? PremiumSince { get; set; }
        public List<PremiumBenefit> PremiumBenefits { get; set; }
        public PremiumStatistics Statistics { get; set; }
    }

    public class PremiumBenefitsViewModel {
        public List<PremiumBenefit> Benefits { get; set; }
        public List<Testimonial> Testimonials { get; set; }
        public List<PricingPlan> PricingPlans { get; set; }
    }

    public class UpgradeViewModel {
        [Required(ErrorMessage = "Selecione um plano")]
        public string SelectedPlan { get; set; }

        [Required(ErrorMessage = "Selecione um método de pagamento")]
        [Display(Name = "Método de Pagamento")]
        public string PaymentMethod { get; set; } = "credit_card";

        [Display(Name = "Email para faturação")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? UserEmail { get; set; }

        public List<PricingPlan> Plans { get; set; }
    }

    public class ManageSubscriptionViewModel {
        public string CurrentPlan { get; set; }
        public DateTime NextBillingDate { get; set; }
        public bool AutoRenew { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class PremiumStatsViewModel {
        public int TotalPremiumUsers { get; set; }
        public int PremiumUsersThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public double ConversionRate { get; set; }
        public List<CategoryPremiumStat> TopCategories { get; set; }
    }

    // Classes auxiliares
    public class PremiumBenefit {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

    public class Testimonial {
        public string Name { get; set; }
        public string Profession { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public string AvatarColor { get; set; }
    }

    public class PricingPlan {
        public string Name { get; set; }
        public string Id { get; set; }
        public decimal Price { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public bool IsPopular { get; set; }
    }

    public class PremiumStatistics {
        public int TotalServices { get; set; }
        public int CompletedServices { get; set; }
        public int CompletionRate { get; set; }
        public int DaysAsPremium { get; set; }
        public decimal EstimatedEarnings { get; set; }
    }

    public class CategoryPremiumStat {
        public string Category { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    // Classe para transações (seria uma tabela no BD)
    public class PremiumTransaction {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string PlanType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
    }
}