using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace EDSG.Controllers {
    [Authorize]
    public class DashboardController : Controller {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardController(AppDbContext context, IHttpContextAccessor httpContextAccessor) {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Dashboard/Index
        public async Task<IActionResult> Index() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var modo = HttpContext.Session.GetString("DashboardMode") ?? "Cliente";
            var viewModel = new DashboardViewModel {
                Modo = modo == "Profissional" ? ModoDashboard.Profissional : ModoDashboard.Cliente
            };

            if (viewModel.Modo == ModoDashboard.Cliente) {
                await CarregarDadosCliente(userId, viewModel);
            } else {
                await CarregarDadosProfissional(userId, viewModel);
            }

            return View(viewModel);
        }

        // POST: Dashboard/SwitchMode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SwitchMode(string mode) {
            HttpContext.Session.SetString("DashboardMode", mode);
            return RedirectToAction(nameof(Index));
        }

        // GET: Dashboard/Mensagens
        public async Task<IActionResult> Mensagens(string? id = null) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscar todas as conversas do usuário
            var conversas = await _context.Mensagens
                .Where(m => (m.RemetenteId == userId || m.DestinatarioId == userId) &&
                           (m.RemetenteId == userId ? !m.DeletedForSender : !m.DeletedForReceiver))
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .OrderByDescending(m => m.DataEnvio)
                .ToListAsync();

            // Agrupar por conversa
            var conversasAgrupadas = conversas
                .GroupBy(m => m.RemetenteId == userId ? m.DestinatarioId : m.RemetenteId)
                .Select(g => new ConversaResumo {
                    OutroUsuarioId = g.Key,
                    OutroUsuario = g.First().RemetenteId == userId
                        ? g.First().Destinatario
                        : g.First().Remetente,
                    UltimaMensagem = g.OrderByDescending(m => m.DataEnvio).First(),
                    MensagensNaoLidas = g.Count(m => !m.IsLida && m.DestinatarioId == userId)
                })
                .OrderByDescending(g => g.UltimaMensagem.DataEnvio)
                .ToList();

            // Se foi especificado um ID, carregar essa conversa
            List<Mensagem> mensagensConversa = null;
            if (!string.IsNullOrEmpty(id)) {
                mensagensConversa = await _context.Mensagens
                    .Where(m => (m.RemetenteId == userId && m.DestinatarioId == id && !m.DeletedForSender) ||
                               (m.RemetenteId == id && m.DestinatarioId == userId && !m.DeletedForReceiver))
                    .Include(m => m.Remetente)
                    .Include(m => m.Destinatario)
                    .OrderBy(m => m.DataEnvio)
                    .ToListAsync();

                // Marcar como lidas
                var mensagensNaoLidas = mensagensConversa
                    .Where(m => m.DestinatarioId == userId && !m.IsLida)
                    .ToList();

                foreach (var mensagem in mensagensNaoLidas) {
                    mensagem.IsLida = true;
                }

                if (mensagensNaoLidas.Any()) {
                    await _context.SaveChangesAsync();
                }
            }

            var viewModel = new MensagensViewModel {
                Conversas = conversasAgrupadas,
                MensagensConversa = mensagensConversa,
                ConversaAtualId = id,
                UsuarioId = userId
            };

            return View(viewModel);
        }

        // POST: Dashboard/EnviarMensagem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensagem(string destinatarioId, string texto) {
            if (string.IsNullOrEmpty(texto)) {
                TempData["ErrorMessage"] = "A mensagem não pode estar vazia.";
                return RedirectToAction(nameof(Mensagens), new { id = destinatarioId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mensagem = new Mensagem {
                RemetenteId = userId,
                DestinatarioId = destinatarioId,
                Texto = texto.Trim(),
                DataEnvio = DateTime.UtcNow,
                IsLida = false
            };

            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mensagem enviada com sucesso!";
            return RedirectToAction(nameof(Mensagens), new { id = destinatarioId });
        }

        // POST: Dashboard/ApagarMensagem/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarMensagem(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var mensagem = await _context.Mensagens.FindAsync(id);

            if (mensagem == null) {
                TempData["ErrorMessage"] = "Mensagem não encontrada.";
                return RedirectToAction(nameof(Mensagens));
            }

            if (mensagem.RemetenteId == userId) {
                mensagem.DeletedForSender = true;
            } else if (mensagem.DestinatarioId == userId) {
                mensagem.DeletedForReceiver = true;
            } else {
                TempData["ErrorMessage"] = "Não tem permissão para apagar esta mensagem.";
                return RedirectToAction(nameof(Mensagens));
            }

            // Se ambos apagaram, remover permanentemente
            if (mensagem.DeletedForSender && mensagem.DeletedForReceiver) {
                _context.Mensagens.Remove(mensagem);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mensagem apagada com sucesso!";
            return RedirectToAction(nameof(Mensagens));
        }

        // POST: Dashboard/ApagarConversa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApagarConversa(string destinatarioId) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mensagens = await _context.Mensagens
                .Where(m => (m.RemetenteId == userId && m.DestinatarioId == destinatarioId) ||
                           (m.RemetenteId == destinatarioId && m.DestinatarioId == userId))
                .ToListAsync();

            if (!mensagens.Any()) {
                TempData["ErrorMessage"] = "Conversa não encontrada.";
                return RedirectToAction(nameof(Mensagens));
            }

            foreach (var mensagem in mensagens) {
                if (mensagem.RemetenteId == userId) {
                    mensagem.DeletedForSender = true;
                } else if (mensagem.DestinatarioId == userId) {
                    mensagem.DeletedForReceiver = true;
                }

                // Se ambos apagaram, remover permanentemente
                if (mensagem.DeletedForSender && mensagem.DeletedForReceiver) {
                    _context.Mensagens.Remove(mensagem);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Conversa apagada com sucesso!";
            return RedirectToAction(nameof(Mensagens));
        }

        // GET: Dashboard/GetMensagensConversa/{id}
        public async Task<IActionResult> GetMensagensConversa(string id) {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mensagens = await _context.Mensagens
                .Where(m => (m.RemetenteId == userId && m.DestinatarioId == id && !m.DeletedForSender) ||
                           (m.RemetenteId == id && m.DestinatarioId == userId && !m.DeletedForReceiver))
                .Include(m => m.Remetente)
                .Include(m => m.Destinatario)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();

            ViewBag.UsuarioId = userId;
            return PartialView("_MensagensPartial", mensagens);
        }

        // GET: Dashboard/NovaConversa
        public async Task<IActionResult> NovaConversa() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscar todos os usuários exceto o atual
            var usuarios = await _context.Users
                .Where(u => u.Id != userId)
                .OrderBy(u => u.Nome)
                .Select(u => new {
                    u.Id,
                    u.Nome,
                    u.FotoPerfil,
                    u.Categoria
                })
                .ToListAsync();

            ViewBag.Usuarios = usuarios;
            ViewBag.UsuarioId = userId;

            return View();
        }

        // GET: Dashboard/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount() {
            if (!User.Identity.IsAuthenticated) {
                return Json(new { count = 0 });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unreadCount = await _context.Mensagens
                .CountAsync(m => m.DestinatarioId == userId && !m.IsLida && !m.DeletedForReceiver);

            return Json(new { count = unreadCount });
        }

        // GET: Dashboard/Servicos
        public async Task<IActionResult> Servicos() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var modo = HttpContext.Session.GetString("DashboardMode") ?? "Cliente";

            List<Servico> servicos;

            if (modo == "Profissional") {
                servicos = await _context.Servicos
                    .Include(s => s.Cliente)
                    .Include(s => s.Profissional)
                    .Where(s => s.ProfissionalId == userId)
                    .OrderByDescending(s => s.DataPedido)
                    .ToListAsync();
            } else {
                servicos = await _context.Servicos
                    .Include(s => s.Cliente)
                    .Include(s => s.Profissional)
                    .Where(s => s.ClienteId == userId)
                    .OrderByDescending(s => s.DataPedido)
                    .ToListAsync();
            }

            ViewBag.Modo = modo;
            return View(servicos);
        }

        // GET: Dashboard/NovoServico/{profissionalId}
        public async Task<IActionResult> NovoServico(string profissionalId) {
            var profissional = await _context.Users.FindAsync(profissionalId);
            if (profissional == null || profissional.Categoria == null) {
                return NotFound();
            }

            var model = new NovoServicoViewModel {
                ProfissionalId = profissionalId,
                ProfissionalNome = profissional.Nome,
                ProfissionalCategoria = profissional.Categoria,
                PrecoSugerido = profissional.PrecoBase ?? 0
            };

            return View(model);
        }

        // POST: Dashboard/NovoServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoServico(NovoServicoViewModel model) {
            if (ModelState.IsValid) {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var servico = new Servico {
                    ClienteId = userId,
                    ProfissionalId = model.ProfissionalId,
                    Titulo = model.Titulo,
                    Descricao = model.Descricao,
                    Categoria = model.Categoria,
                    Localizacao = model.Localizacao,
                    PrecoAcordado = model.Preco,
                    Estado = EstadoServico.Pendente,
                    DataPedido = DateTime.UtcNow
                };

                _context.Servicos.Add(servico);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Pedido de serviço enviado com sucesso!";
                return RedirectToAction(nameof(Servicos));
            }

            // Recarregar dados do profissional se necessário
            if (!string.IsNullOrEmpty(model.ProfissionalId)) {
                var profissional = await _context.Users.FindAsync(model.ProfissionalId);
                if (profissional != null) {
                    model.ProfissionalNome = profissional.Nome;
                    model.ProfissionalCategoria = profissional.Categoria;
                }
            }

            return View(model);
        }

        // POST: Dashboard/AceitarServico/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AceitarServico(int id) {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (servico.ProfissionalId != userId) {
                return Forbid();
            }

            servico.Estado = EstadoServico.Aceite;
            servico.DataAceitacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço aceite com sucesso!";
            return RedirectToAction(nameof(Servicos));
        }

        // POST: Dashboard/RecusarServico/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecusarServico(int id) {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (servico.ProfissionalId != userId) {
                return Forbid();
            }

            servico.Estado = EstadoServico.Recusado;
            await _context.SaveChangesAsync();

            TempData["InfoMessage"] = "Serviço recusado.";
            return RedirectToAction(nameof(Servicos));
        }

        // POST: Dashboard/ConcluirServico/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConcluirServico(int id) {
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (servico.ProfissionalId != userId) {
                return Forbid();
            }

            servico.Estado = EstadoServico.Concluido;
            servico.DataConclusao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço marcado como concluído!";
            return RedirectToAction(nameof(Servicos));
        }

        // GET: Dashboard/AvaliarServico/{id}
        public async Task<IActionResult> AvaliarServico(int id) {
            var servico = await _context.Servicos
                .Include(s => s.Profissional)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null || servico.Estado != EstadoServico.Concluido) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (servico.ClienteId != userId) {
                return Forbid();
            }

            // Verificar se já existe avaliação
            var avaliacaoExistente = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.ServicoId == id);

            var model = new AvaliarServicoViewModel {
                ServicoId = id,
                ProfissionalNome = servico.Profissional.Nome,
                Nota = avaliacaoExistente?.Nota ?? 0,
                Comentario = avaliacaoExistente?.Comentario
            };

            return View(model);
        }

        // POST: Dashboard/AvaliarServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvaliarServico(AvaliarServicoViewModel model) {
            if (ModelState.IsValid) {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var servico = await _context.Servicos.FindAsync(model.ServicoId);

                if (servico == null || servico.ClienteId != userId) {
                    return Forbid();
                }

                var avaliacaoExistente = await _context.Avaliacoes
                    .FirstOrDefaultAsync(a => a.ServicoId == model.ServicoId);

                if (avaliacaoExistente == null) {
                    var avaliacao = new Avaliacao {
                        ServicoId = model.ServicoId,
                        AvaliadorId = userId,
                        AvaliadoId = servico.ProfissionalId,
                        Nota = model.Nota,
                        Comentario = model.Comentario,
                        DataAvaliacao = DateTime.UtcNow
                    };

                    _context.Avaliacoes.Add(avaliacao);
                } else {
                    avaliacaoExistente.Nota = model.Nota;
                    avaliacaoExistente.Comentario = model.Comentario;
                    avaliacaoExistente.DataAvaliacao = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Avaliação enviada com sucesso!";
                return RedirectToAction(nameof(Servicos));
            }

            return View(model);
        }

        // GET: Dashboard/DetalhesServico/{id}
        public async Task<IActionResult> DetalhesServico(int id) {
            var servico = await _context.Servicos
                .Include(s => s.Cliente)
                .Include(s => s.Profissional)
                .Include(s => s.Avaliacao)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var modo = HttpContext.Session.GetString("DashboardMode") ?? "Cliente";

            // Verificar permissões
            if (servico.ClienteId != userId && servico.ProfissionalId != userId && !User.IsInRole("Admin")) {
                return Forbid();
            }

            ViewBag.Modo = modo;
            ViewBag.UserId = userId;
            ViewBag.IsCliente = servico.ClienteId == userId;
            ViewBag.IsProfissional = servico.ProfissionalId == userId;

            return View(servico);
        }

        // GET: Dashboard/EditarServico/{id}
        public async Task<IActionResult> EditarServico(int id) {
            var servico = await _context.Servicos.FindAsync(id);

            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Apenas o cliente pode editar serviços pendentes
            if (servico.ClienteId != userId || servico.Estado != EstadoServico.Pendente) {
                return Forbid();
            }

            var model = new EditarServicoViewModel {
                Id = servico.Id,
                Titulo = servico.Titulo,
                Descricao = servico.Descricao,
                Categoria = servico.Categoria,
                Localizacao = servico.Localizacao,
                PrecoAcordado = servico.PrecoAcordado
            };

            return View(model);
        }

        // POST: Dashboard/EditarServico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarServico(EditarServicoViewModel model) {
            if (ModelState.IsValid) {
                var servico = await _context.Servicos.FindAsync(model.Id);

                if (servico == null) {
                    return NotFound();
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (servico.ClienteId != userId || servico.Estado != EstadoServico.Pendente) {
                    return Forbid();
                }

                servico.Titulo = model.Titulo;
                servico.Descricao = model.Descricao;
                servico.Categoria = model.Categoria;
                servico.Localizacao = model.Localizacao;
                servico.PrecoAcordado = model.PrecoAcordado;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Serviço atualizado com sucesso!";
                return RedirectToAction(nameof(DetalhesServico), new { id = servico.Id });
            }

            return View(model);
        }

        // POST: Dashboard/CancelarServico/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarServico(int id, string motivo = null) {
            var servico = await _context.Servicos.FindAsync(id);

            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Cliente pode cancelar serviços pendentes
            // Profissional pode cancelar serviços que aceitou
            // Ambos podem cancelar serviços em progresso
            if (servico.ClienteId != userId && servico.ProfissionalId != userId) {
                return Forbid();
            }

            // Verificar se o serviço pode ser cancelado
            if (servico.Estado == EstadoServico.Concluido || servico.Estado == EstadoServico.Recusado) {
                TempData["ErrorMessage"] = "Este serviço não pode ser cancelado no estado atual.";
                return RedirectToAction(nameof(DetalhesServico), new { id });
            }

            servico.Estado = EstadoServico.Cancelado;

            // Adicionar motivo se fornecido
            if (!string.IsNullOrEmpty(motivo)) {
                servico.RespostaProfissional = $"Cancelado por {(servico.ClienteId == userId ? "cliente" : "profissional")}: {motivo}";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço cancelado com sucesso!";
            return RedirectToAction(nameof(Servicos));
        }

        // POST: Dashboard/IniciarServico/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarServico(int id) {
            var servico = await _context.Servicos.FindAsync(id);

            if (servico == null) {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (servico.ProfissionalId != userId) {
                return Forbid();
            }

            if (servico.Estado != EstadoServico.Aceite) {
                TempData["ErrorMessage"] = "Apenas serviços aceites podem ser iniciados.";
                return RedirectToAction(nameof(DetalhesServico), new { id });
            }

            servico.Estado = EstadoServico.EmProgresso;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço iniciado!";
            return RedirectToAction(nameof(DetalhesServico), new { id });
        }

        // GET: Dashboard/Favoritos
        public async Task<IActionResult> Favoritos() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favoritos = await _context.Favoritos
                .Where(f => f.ClienteId == userId)
                .Include(f => f.Profissional)
                .ToListAsync();

            var profissionais = favoritos.Select(f => f.Profissional).ToList();

            // Calcular avaliação média para cada profissional
            foreach (var profissional in profissionais) {
                profissional.AvaliacoesRecebidas = await _context.Avaliacoes
                    .Where(a => a.AvaliadoId == profissional.Id)
                    .ToListAsync();

                if (profissional.AvaliacoesRecebidas.Any()) {
                    profissional.AvaliacaoMedia = profissional.AvaliacoesRecebidas.Average(a => a.Nota);
                }
            }

            return View(profissionais);
        }

        // POST: Dashboard/AdicionarFavorito/{profissionalId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarFavorito(string profissionalId) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar se já é favorito
            var favoritoExistente = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.ClienteId == userId && f.ProfissionalId == profissionalId);

            if (favoritoExistente == null) {
                var favorito = new Favorito {
                    ClienteId = userId,
                    ProfissionalId = profissionalId,
                    DataAdicao = DateTime.UtcNow
                };

                _context.Favoritos.Add(favorito);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profissional adicionado aos favoritos!";
            }

            return RedirectToAction(nameof(Favoritos));
        }

        // POST: Dashboard/RemoverFavorito/{profissionalId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverFavorito(string profissionalId) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.ClienteId == userId && f.ProfissionalId == profissionalId);

            if (favorito != null) {
                _context.Favoritos.Remove(favorito);
                await _context.SaveChangesAsync();

                TempData["InfoMessage"] = "Profissional removido dos favoritos.";
            }

            return RedirectToAction(nameof(Favoritos));
        }

        // Métodos auxiliares privados
        private async Task CarregarDadosCliente(string userId, DashboardViewModel viewModel) {
            // Serviços do cliente
            viewModel.ServicosPendentes = await _context.Servicos
                .Include(s => s.Profissional)
                .Where(s => s.ClienteId == userId && s.Estado == EstadoServico.Pendente)
                .OrderByDescending(s => s.DataPedido)
                .Take(5)
                .ToListAsync();

            viewModel.ServicosAtivos = await _context.Servicos
                .Include(s => s.Profissional)
                .Where(s => s.ClienteId == userId &&
                           (s.Estado == EstadoServico.Aceite || s.Estado == EstadoServico.EmProgresso))
                .OrderByDescending(s => s.DataAceitacao)
                .Take(5)
                .ToListAsync();

            viewModel.ServicosConcluidos = await _context.Servicos
                .Include(s => s.Profissional)
                .Where(s => s.ClienteId == userId && s.Estado == EstadoServico.Concluido)
                .OrderByDescending(s => s.DataConclusao)
                .Take(5)
                .ToListAsync();

            // Mensagens não lidas
            viewModel.MensagensRecebidas = await _context.Mensagens
                .Include(m => m.Remetente)
                .Where(m => m.DestinatarioId == userId && !m.IsLida && !m.DeletedForReceiver)
                .OrderByDescending(m => m.DataEnvio)
                .Take(10)
                .ToListAsync();

            // Favoritos
            var favoritosIds = await _context.Favoritos
                .Where(f => f.ClienteId == userId)
                .Select(f => f.ProfissionalId)
                .ToListAsync();

            viewModel.Favoritos = await _context.Users
                .Where(u => favoritosIds.Contains(u.Id))
                .Take(6)
                .ToListAsync();
        }

        private async Task CarregarDadosProfissional(string userId, DashboardViewModel viewModel) {
            // Serviços do profissional
            viewModel.ServicosPendentes = await _context.Servicos
                .Include(s => s.Cliente)
                .Where(s => s.ProfissionalId == userId && s.Estado == EstadoServico.Pendente)
                .OrderByDescending(s => s.DataPedido)
                .Take(5)
                .ToListAsync();

            viewModel.ServicosAtivos = await _context.Servicos
                .Include(s => s.Cliente)
                .Where(s => s.ProfissionalId == userId &&
                           (s.Estado == EstadoServico.Aceite || s.Estado == EstadoServico.EmProgresso))
                .OrderByDescending(s => s.DataAceitacao)
                .Take(5)
                .ToListAsync();

            viewModel.ServicosConcluidos = await _context.Servicos
                .Include(s => s.Cliente)
                .Where(s => s.ProfissionalId == userId && s.Estado == EstadoServico.Concluido)
                .OrderByDescending(s => s.DataConclusao)
                .Take(5)
                .ToListAsync();

            // Mensagens não lidas
            viewModel.MensagensRecebidas = await _context.Mensagens
                .Include(m => m.Remetente)
                .Where(m => m.DestinatarioId == userId && !m.IsLida && !m.DeletedForReceiver)
                .OrderByDescending(m => m.DataEnvio)
                .Take(10)
                .ToListAsync();

            // Estatísticas
            viewModel.Estatisticas = new EstatisticasProfissional {
                TotalServicos = await _context.Servicos
                    .CountAsync(s => s.ProfissionalId == userId),
                ServicosConcluidos = await _context.Servicos
                    .CountAsync(s => s.ProfissionalId == userId && s.Estado == EstadoServico.Concluido),
                ServicosPendentes = await _context.Servicos
                    .CountAsync(s => s.ProfissionalId == userId && s.Estado == EstadoServico.Pendente),
                AvaliacaoMedia = await _context.Avaliacoes
                    .Where(a => a.AvaliadoId == userId)
                    .Select(a => (double?)a.Nota)
                    .DefaultIfEmpty()
                    .AverageAsync() ?? 0,
                ReceitaTotal = (decimal)((await _context.Servicos
                    .Where(s => s.ProfissionalId == userId && s.Estado == EstadoServico.Concluido)
                    .Select(s => (double?)s.PrecoAcordado)
                    .SumAsync()) ?? 0)
            };

            // Avaliações recebidas recentes
            viewModel.AvaliacoesRecebidas = await _context.Avaliacoes
                .Include(a => a.Avaliador)
                .Where(a => a.AvaliadoId == userId)
                .OrderByDescending(a => a.DataAvaliacao)
                .Take(5)
                .ToListAsync();
        }
    }
}