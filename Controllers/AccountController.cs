using EDSG.Data;
using EDSG.Models;
using EDSG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;

namespace EDSG.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly CategoriaHelper _categoriaHelper;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            AppDbContext context,
            IEmailService emailService) {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _categoriaHelper = new CategoriaHelper(context);
        }

        // GET: Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid) {
                var user = new ApplicationUser {
                    UserName = model.Email,
                    Email = model.Email,
                    Nome = model.Nome,
                    Localizacao = model.Localizacao,
                    IsActive = true,
                    DataRegistro = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) {
                    // Adicionar ao role Cliente por padrão
                    await _userManager.AddToRoleAsync(user, "Client");

                    // Enviar email de boas-vindas
                    try {
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.Nome);
                        TempData["SuccessMessage"] = "Conta criada com sucesso! Um email de boas-vindas foi enviado.";
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Erro ao enviar email de boas-vindas para {Email}", user.Email);
                        TempData["SuccessMessage"] = "Conta criada com sucesso!";
                    }

                    // Login automático
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Definir modo inicial como Cliente
                    HttpContext.Session.SetString("DashboardMode", "Cliente");

                    _logger.LogInformation("Novo utilizador registado: {Email}", user.Email);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null) {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null) {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid) {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded) {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && !user.IsActive) {
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Esta conta está desativada.");
                        return View(model);
                    }

                    // Definir modo inicial como Cliente
                    HttpContext.Session.SetString("DashboardMode", "Cliente");

                    _logger.LogInformation("Utilizador autenticado: {Email}", model.Email);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email ou palavra-passe incorretos.");
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            _logger.LogInformation("Utilizador deslogado.");
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound();
            }

            // Carregar categorias existentes para sugestões
            ViewBag.CategoriasExistentes = await _categoriaHelper.GetCategoriasExistentes();

            var model = new ProfileViewModel {
                Nome = user.Nome,
                Email = user.Email,
                Localizacao = user.Localizacao,
                Categoria = user.Categoria,
                Especialidade = user.Especialidade,
                PrecoBase = user.PrecoBase,
                Bio = user.Bio,
                FotoPerfilUrl = user.FotoPerfil,
                PortfolioFilePath = user.PortfolioFile,
                PortfolioFileName = user.PortfolioFileName
            };

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    return NotFound();
                }

                user.Nome = model.Nome;
                user.Localizacao = model.Localizacao;
                user.Categoria = model.Categoria;
                user.Especialidade = model.Especialidade;
                user.PrecoBase = model.PrecoBase;
                user.Bio = model.Bio;
                user.UltimaAtualizacao = DateTime.UtcNow;

                // Adicionar nova categoria se não existir
                if (!string.IsNullOrEmpty(model.Categoria)) {
                    await _categoriaHelper.AddCategoria(model.Categoria);
                }

                // Verificar se o usuário quer remover a foto
                var removePhoto = Request.Form["RemovePhoto"] == "true";

                if (removePhoto && !string.IsNullOrEmpty(user.FotoPerfil)) {
                    // Remover arquivo físico
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) {
                        System.IO.File.Delete(oldFilePath);
                    }
                    user.FotoPerfil = null;
                }
                // Processar upload da nova foto
                else if (model.FotoPerfil != null && model.FotoPerfil.Length > 0) {
                    try {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile");

                        // Criar diretório se não existir
                        if (!Directory.Exists(uploadsFolder)) {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Gerar nome único para o arquivo
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.FotoPerfil.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Salvar arquivo
                        using (var stream = new FileStream(filePath, FileMode.Create)) {
                            await model.FotoPerfil.CopyToAsync(stream);
                        }

                        // Remover foto antiga se existir
                        if (!string.IsNullOrEmpty(user.FotoPerfil)) {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.FotoPerfil.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Atualizar caminho da foto no banco de dados
                        user.FotoPerfil = $"/uploads/profile/{uniqueFileName}";
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Erro ao fazer upload da foto de perfil");
                        TempData["ErrorMessage"] = "Erro ao fazer upload da foto. Tente novamente.";
                    }
                }

                // Verificar se o usuário quer remover o portfólio
                var removePortfolio = Request.Form["RemovePortfolio"] == "true";

                if (removePortfolio && !string.IsNullOrEmpty(user.PortfolioFile)) {
                    // Remover arquivo físico
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PortfolioFile.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) {
                        System.IO.File.Delete(oldFilePath);
                    }
                    user.PortfolioFile = null;
                    user.PortfolioFileName = null;
                }
                // Processar upload do portfólio PDF
                else if (model.PortfolioFile != null && model.PortfolioFile.Length > 0) {
                    try {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "portfolio");

                        // Criar diretório se não existir
                        if (!Directory.Exists(uploadsFolder)) {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Gerar nome único para o arquivo
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.PortfolioFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Salvar arquivo
                        using (var stream = new FileStream(filePath, FileMode.Create)) {
                            await model.PortfolioFile.CopyToAsync(stream);
                        }

                        // Remover arquivo antigo se existir
                        if (!string.IsNullOrEmpty(user.PortfolioFile)) {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PortfolioFile.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath)) {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Atualizar caminho do portfólio no banco de dados
                        user.PortfolioFile = $"/uploads/portfolio/{uniqueFileName}";
                        user.PortfolioFileName = model.PortfolioFile.FileName;
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Erro ao fazer upload do portfólio");
                        TempData["ErrorMessage"] = "Erro ao fazer upload do portfólio. Tente novamente.";
                    }
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded) {
                    TempData["SuccessMessage"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Recarregar as URLs dos arquivos para exibição
            var currentUser = await _userManager.GetUserAsync(User);
            model.FotoPerfilUrl = currentUser?.FotoPerfil;
            model.PortfolioFilePath = currentUser?.PortfolioFile;
            model.PortfolioFileName = currentUser?.PortfolioFileName;

            return View(model);
        }

        // GET: Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    return NotFound();
                }

                var result = await _userManager.ChangePasswordAsync(
                    user, model.CurrentPassword, model.NewPassword);

                if (result.Succeeded) {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Palavra-passe alterada com sucesso!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/ChangeEmail
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeEmail() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound();
            }

            var model = new ChangeEmailViewModel {
                CurrentEmail = user.Email
            };

            return View(model);
        }

        // POST: Account/ChangeEmail
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    return NotFound();
                }

                // Verificar se o email já está em uso por outro usuário
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);
                if (existingUser != null && existingUser.Id != user.Id) {
                    ModelState.AddModelError("NewEmail", "Este email já está em uso por outra conta.");
                    return View(model);
                }

                // Gerar token de confirmação
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);

                // Alterar email
                var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);

                if (result.Succeeded) {
                    // Atualizar nome de usuário também
                    user.UserName = model.NewEmail;
                    await _userManager.UpdateAsync(user);

                    await _signInManager.RefreshSignInAsync(user);

                    TempData["SuccessMessage"] = "Email alterado com sucesso!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) {
                    // Não revelar que o usuário não existe por segurança
                    TempData["SuccessMessage"] = "Se o email estiver registado, enviaremos um link de recuperação.";
                    return RedirectToAction(nameof(ForgotPassword));
                }

                // Gerar token de recuperação
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { email = user.Email, token = token },
                    protocol: HttpContext.Request.Scheme);

                try {
                    await _emailService.SendPasswordResetEmailAsync(user.Email, callbackUrl);
                    TempData["SuccessMessage"] = "Link de recuperação enviado para seu email. Verifique sua caixa de entrada.";
                    return RedirectToAction(nameof(ForgotPassword));
                } catch (Exception ex) {
                    _logger.LogError(ex, "Erro ao enviar email de recuperação para {Email}", user.Email);
                    ModelState.AddModelError(string.Empty, "Erro ao enviar email. Tente novamente mais tarde.");
                }
            }

            return View(model);
        }

        // GET: Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token) {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) {
                TempData["ErrorMessage"] = "Link de recuperação inválido ou expirado.";
                return RedirectToAction(nameof(Login));
            }

            var model = new ResetPasswordViewModel {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) {
                // Usuário não encontrado
                TempData["ErrorMessage"] = "Utilizador não encontrado.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) {
                TempData["SuccessMessage"] = "Palavra-passe redefinida com sucesso! Agora pode fazer login com sua nova palavra-passe.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Account/ViewPortfolio
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ViewPortfolio() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.PortfolioFile)) {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PortfolioFile.TrimStart('/'));

            if (!System.IO.File.Exists(filePath)) {
                return NotFound();
            }

            // Retornar o PDF para visualização inline
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf");
        }

        // GET: Account/ConfirmDeleteAccount
        [HttpGet]
        [Authorize]
        public IActionResult ConfirmDeleteAccount() {
            return View();
        }

        // POST: Account/ConfirmDeleteAccount
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteAccount(DeleteAccountViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) {
                    return NotFound();
                }

                // Verificar password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid) {
                    ModelState.AddModelError("Password", "Password incorreta.");
                    return View(model);
                }

                // Marcar conta como inativa
                user.IsActive = false;
                user.Email = $"deleted_{user.Id}@example.com";
                user.UserName = $"deleted_{user.Id}";
                user.Nome = "Utilizador Eliminado";
                user.Localizacao = null;
                user.Categoria = null;
                user.Especialidade = null;
                user.PrecoBase = null;
                user.Bio = null;
                user.FotoPerfil = null;
                user.PortfolioFile = null;
                user.PortfolioFileName = null;
                user.UltimaAtualizacao = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded) {
                    // Fazer logout
                    await _signInManager.SignOutAsync();
                    HttpContext.Session.Clear();

                    _logger.LogWarning("Conta eliminada: {UserId}", user.Id);

                    TempData["SuccessMessage"] = "Sua conta foi eliminada com sucesso.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: Account/DeleteAccount
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(string password) {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound();
            }

            // Verificar password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid) {
                TempData["ErrorMessage"] = "Password incorreta.";
                return RedirectToAction(nameof(Profile));
            }

            // Marcar conta como inativa
            user.IsActive = false;
            user.Email = $"deleted_{user.Id}@example.com";
            user.UserName = $"deleted_{user.Id}";
            user.Nome = "Utilizador Eliminado";

            await _userManager.UpdateAsync(user);

            // Fazer logout
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            _logger.LogWarning("Conta eliminada via API: {UserId}", user.Id);

            TempData["SuccessMessage"] = "Sua conta foi eliminada com sucesso.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/GetCategorias
        [HttpGet]
        public async Task<IActionResult> GetCategorias() {
            var categorias = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Categoria))
                .Select(u => u.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Json(categorias);
        }

        // POST: Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null) {
            try {
                // Gerar URL de redirecionamento
                var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

                // Configurar propriedades de autenticação
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

                _logger.LogInformation("Iniciando login externo com {Provider}", provider);

                // Retornar Challenge (redireciona para o provedor)
                return Challenge(properties, provider);
            } catch (Exception ex) {
                _logger.LogError(ex, "Erro ao iniciar login externo com {Provider}", provider);
                TempData["ErrorMessage"] = "Erro ao iniciar autenticação com Google. Tente novamente.";
                return RedirectToAction(nameof(Login));
            }
        }

        // GET: Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null) {
            if (remoteError != null) {
                _logger.LogError("Erro do provedor externo: {Error}", remoteError);
                TempData["ErrorMessage"] = $"Erro do provedor de autenticação: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                _logger.LogWarning("Informações de login externo não disponíveis.");
                TempData["ErrorMessage"] = "Não foi possível obter informações do provedor de autenticação.";
                return RedirectToAction(nameof(Login));
            }

            // Tentar fazer login com as credenciais externas
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded) {
                // Login bem-sucedido
                _logger.LogInformation("Utilizador autenticado com {Provider}", info.LoginProvider);
                HttpContext.Session.SetString("DashboardMode", "Cliente");
                TempData["SuccessMessage"] = "Login realizado com sucesso!";
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut) {
                _logger.LogWarning("Conta bloqueada tentando login com {Provider}", info.LoginProvider);
                return View("Lockout");
            } else {
                // Se não existir, criar conta automaticamente
                return await CreateExternalUser(info, returnUrl);
            }
        }

        private async Task<IActionResult> CreateExternalUser(ExternalLoginInfo info, string returnUrl) {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(email)) {
                _logger.LogError("Email não disponível nas claims do Google.");
                TempData["ErrorMessage"] = "Não foi possível obter seu email do Google.";
                return RedirectToAction(nameof(Login));
            }

            // Verificar se usuário já existe pelo email
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null) {
                // Adicionar login externo à conta existente
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded) {
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    TempData["SuccessMessage"] = "Login com Google adicionado à sua conta existente!";
                    return RedirectToLocal(returnUrl);
                }
            }

            // Criar novo usuário
            var user = new ApplicationUser {
                UserName = email,
                Email = email,
                Nome = name ?? email.Split('@')[0],
                Localizacao = "Portugal", // Localização padrão
                IsActive = true,
                DataRegistro = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded) {
                // Adicionar login externo
                await _userManager.AddLoginAsync(user, info);

                // Adicionar ao role Cliente
                await _userManager.AddToRoleAsync(user, "Client");

                // Fazer login
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Enviar email de boas-vindas
                try {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Nome);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Erro ao enviar email de boas-vindas");
                }

                _logger.LogInformation("Novo utilizador criado com Google: {Email}", user.Email);
                HttpContext.Session.SetString("DashboardMode", "Cliente");
                TempData["SuccessMessage"] = "Conta criada e login realizado com sucesso!";
                return RedirectToLocal(returnUrl);
            } else {
                _logger.LogError("Erro ao criar utilizador: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Erro ao criar conta. Tente novamente.";
                return RedirectToAction(nameof(Login));
            }
        }

        // GET: Account/ExternalLoginFailure
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLoginFailure() {
            return View();
        }

        // GET: Account/SwitchLanguage
        [HttpGet]
        public IActionResult SwitchLanguage(string culture, string returnUrl) {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied() {
            return View();
        }

        // Helper Methods
        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            } else {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }

    // Helper para categorias
    public class CategoriaHelper {
        private readonly AppDbContext _context;

        public CategoriaHelper(AppDbContext context) {
            _context = context;
        }

        public async Task<List<string>> GetCategoriasExistentes() {
            return await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Categoria))
                .Select(u => u.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task AddCategoria(string categoria) {
            if (!string.IsNullOrEmpty(categoria)) {
                // Aqui você pode implementar lógica para armazenar categorias
                // Por exemplo, em uma tabela separada se necessário
                // Por enquanto, apenas registramos no log
                Console.WriteLine($"Categoria usada: {categoria}");

                // Opcional: Armazenar em uma tabela de categorias
                /*
                var categoriaExistente = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Nome == categoria);
                
                if (categoriaExistente == null) {
                    _context.Categorias.Add(new Categoria { Nome = categoria });
                    await _context.SaveChangesAsync();
                }
                */
            }
        }

        public async Task<bool> CategoriaExiste(string categoria) {
            return await _context.Users
                .AnyAsync(u => u.Categoria == categoria);
        }
    }

    // ViewModels
    public class RegisterViewModel {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Localização")]
        [StringLength(200, ErrorMessage = "A localização não pode ter mais de 200 caracteres")]
        public string? Localizacao { get; set; }

        [Required(ErrorMessage = "A palavra-passe é obrigatória")]
        [StringLength(100, ErrorMessage = "A palavra-passe deve ter entre {2} e {1} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar palavra-passe")]
        [Compare("Password", ErrorMessage = "As palavras-passe não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A palavra-passe é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }

    public class ProfileViewModel {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Localização")]
        [StringLength(200, ErrorMessage = "A localização não pode ter mais de 200 caracteres")]
        public string? Localizacao { get; set; }

        [Display(Name = "Categoria Profissional")]
        [StringLength(100, ErrorMessage = "A categoria não pode ter mais de 100 caracteres")]
        public string? Categoria { get; set; }

        [Display(Name = "Especialidade")]
        [StringLength(200, ErrorMessage = "A especialidade não pode ter mais de 200 caracteres")]
        public string? Especialidade { get; set; }

        [Display(Name = "Preço Base (€/hora)")]
        [Range(0, 1000, ErrorMessage = "O preço deve estar entre 0 e 1000")]
        public decimal? PrecoBase { get; set; }

        [Display(Name = "Biografia")]
        [StringLength(2000, ErrorMessage = "A biografia não pode ter mais de 2000 caracteres")]
        public string? Bio { get; set; }

        [Display(Name = "Portfólio (PDF)")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".pdf" })]
        [MaxFileSize(10 * 1024 * 1024)] // 10MB
        public IFormFile? PortfolioFile { get; set; }

        public string? PortfolioFilePath { get; set; }
        public string? PortfolioFileName { get; set; }

        [Display(Name = "Foto de Perfil")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif" })]
        [MaxFileSize(5 * 1024 * 1024)] // 5MB
        public IFormFile? FotoPerfil { get; set; }

        public string? FotoPerfilUrl { get; set; }
    }

    public class ChangePasswordViewModel {
        [Required(ErrorMessage = "A palavra-passe atual é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova palavra-passe é obrigatória")]
        [StringLength(100, ErrorMessage = "A palavra-passe deve ter entre {2} e {1} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova palavra-passe")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova palavra-passe")]
        [Compare("NewPassword", ErrorMessage = "As palavras-passe não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangeEmailViewModel {
        [Required(ErrorMessage = "O email atual é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email Atual")]
        public string CurrentEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "O novo email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Novo Email")]
        public string NewEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme o novo email")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Confirmar Novo Email")]
        [Compare("NewEmail", ErrorMessage = "Os emails não coincidem")]
        public string ConfirmNewEmail { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova palavra-passe é obrigatória")]
        [StringLength(100, ErrorMessage = "A palavra-passe deve ter entre {2} e {1} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova palavra-passe")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar palavra-passe")]
        [Compare("NewPassword", ErrorMessage = "As palavras-passe não coincidem")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }

    public class DeleteAccountViewModel {
        [Required(ErrorMessage = "A palavra-passe é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirmo que compreendo que esta ação é irreversível")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Deve confirmar que compreende")]
        public bool Confirmation { get; set; }
    }

    // Classes de validação para upload de arquivos
    public class AllowedExtensionsAttribute : ValidationAttribute {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions) {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is IFormFile file) {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(extension)) {
                    return new ValidationResult($"Apenas ficheiros {string.Join(", ", _extensions)} são permitidos.");
                }
            }
            return ValidationResult.Success;
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize) {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is IFormFile file) {
                if (file.Length > _maxFileSize) {
                    return new ValidationResult($"O tamanho máximo do ficheiro é {_maxFileSize / (1024 * 1024)}MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}