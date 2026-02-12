using EDSG.Data;
using EDSG.Models;
using EDSG.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add logging for debugging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();

// Add SQLite Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add Google Authentication
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

Console.WriteLine($"Google Client ID configurado: {!string.IsNullOrEmpty(googleClientId)}");
Console.WriteLine($"Google Client Secret configurado: {!string.IsNullOrEmpty(googleClientSecret)}");

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication(options => {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddGoogle(googleOptions => {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
        googleOptions.CallbackPath = "/signin-google";
        googleOptions.SaveTokens = true;

        // Configurar scopes
        googleOptions.Scope.Add("email");
        googleOptions.Scope.Add("profile");
        googleOptions.Scope.Add("openid");

        // Mapear claims
        googleOptions.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
        googleOptions.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
        googleOptions.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id", "string");

        // Configurar para sempre pedir seleção de conta
        googleOptions.Events.OnRedirectToAuthorizationEndpoint = context => {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account&access_type=offline");
            return Task.CompletedTask;
        };

        googleOptions.Events.OnCreatingTicket = context => {
            Console.WriteLine($"Token de acesso recebido: {context.AccessToken != null}");
            Console.WriteLine($"Email do usuário: {context.Identity?.FindFirst(ClaimTypes.Email)?.Value}");
            return Task.CompletedTask;
        };

        googleOptions.Events.OnRemoteFailure = context => {
            Console.WriteLine($"Erro no login Google: {context.Failure?.Message}");
            context.Response.Redirect("/Account/Login?error=google_failed");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });

    Console.WriteLine("Autenticação Google configurada com sucesso.");
}
else
{
    Console.WriteLine("⚠️ ATENÇÃO: Credenciais do Google não configuradas. Login com Google não estará disponível.");
}

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new[]
    {
        new CultureInfo("pt-PT"),
        new CultureInfo("en-US")
    };

    options.DefaultRequestCulture = new RequestCulture("pt-PT");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// Add Session
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add other services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add CORS if needed
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    Console.WriteLine("Ambiente de desenvolvimento ativado.");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    Console.WriteLine("Ambiente de produção ativado.");
}

app.UseHttpsRedirection();

// Configurar para servir arquivos estáticos
app.UseStaticFiles();

// Crie a pasta para uploads se não existir
var uploadsProfilePath = Path.Combine(app.Environment.WebRootPath, "uploads", "profile");
if (!Directory.Exists(uploadsProfilePath))
{
    Directory.CreateDirectory(uploadsProfilePath);
    Console.WriteLine($"Pasta de uploads de perfil criada: {uploadsProfilePath}");
}

// Crie a pasta para portfólio se não existir
var uploadsPortfolioPath = Path.Combine(app.Environment.WebRootPath, "uploads", "portfolio");
if (!Directory.Exists(uploadsPortfolioPath))
{
    Directory.CreateDirectory(uploadsPortfolioPath);
    Console.WriteLine($"Pasta de uploads de portfólio criada: {uploadsPortfolioPath}");
}

// Criar também a pasta wwwroot se não existir
var wwwrootPath = app.Environment.WebRootPath;
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"Pasta wwwroot criada: {wwwrootPath}");
}

// Criar pasta css se não existir
var cssPath = Path.Combine(wwwrootPath, "css");
if (!Directory.Exists(cssPath))
{
    Directory.CreateDirectory(cssPath);
    Console.WriteLine($"Pasta css criada: {cssPath}");
}

app.UseRouting();

// IMPORTANTE: UseCors antes de UseAuthentication
app.UseCors("AllowAll");

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// IMPORTANTE: UseAuthentication deve vir antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Log middleware pipeline
app.Use(async (context, next) => {
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================
// INICIALIZAÇÃO DO BANCO DE DADOS - SIMPLIFICADA
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("=== INICIANDO INICIALIZAÇÃO DO BANCO DE DADOS ===");

        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // 1. Garantir que o banco de dados seja criado
        Console.WriteLine("Criando/verificando banco de dados...");
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✓ Banco de dados verificado/criado.");

        // 2. Verificar conexão
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"✓ Conexão com banco: {(canConnect ? "OK" : "FALHA")}");

        if (!canConnect)
        {
            throw new Exception("Não foi possível conectar ao banco de dados");
        }

        // 3. Verificar tabelas principais
        var tables = new[] {
            "AspNetUsers", "AspNetRoles", "AspNetUserRoles",
            "Servicos", "Mensagens", "Avaliacoes",
            "Favoritos", "Denuncias", "ServicosProfissionais", "PortfolioItems"
        };

        foreach (var table in tables)
        {
            try
            {
                var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}'";
                var exists = await context.Database.ExecuteSqlRawAsync(sql) > 0;
                Console.WriteLine($"Tabela '{table}': {(exists ? "✓ EXISTE" : "✗ NÃO EXISTE")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar tabela {table}: {ex.Message}");
            }
        }

        // 4. Executar Seed Data
        Console.WriteLine("Executando seed de dados...");
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedData.Initialize(context, userManager, roleManager);

        // 5. Verificar dados inseridos
        var userCount = await context.Users.CountAsync();
        var serviceCount = await context.Servicos.CountAsync();
        var messageCount = await context.Mensagens.CountAsync();

        Console.WriteLine("=== DADOS INSERIDOS ===");
        Console.WriteLine($"✓ Usuários: {userCount}");
        Console.WriteLine($"✓ Serviços: {serviceCount}");
        Console.WriteLine($"✓ Mensagens: {messageCount}");
        Console.WriteLine("=== SEED CONCLUÍDO ===");

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "ERRO ao inicializar a base de dados.");
        Console.WriteLine($"═══ ERRO DETALHADO ═══");
        Console.WriteLine($"Mensagem: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        Console.WriteLine($"══════════════════════");
    }
}

// Log startup information
Console.WriteLine("=========================================");
Console.WriteLine("EDSG Platform iniciando...");
Console.WriteLine($"Ambiente: {app.Environment.EnvironmentName}");
Console.WriteLine($"URLs disponíveis:");
Console.WriteLine($"  - https://localhost:5001");
Console.WriteLine($"  - https://localhost:5000");
Console.WriteLine($"WebRootPath: {app.Environment.WebRootPath}");
Console.WriteLine($"Pasta de uploads de perfil: {uploadsProfilePath}");
Console.WriteLine($"Pasta de uploads de portfólio: {uploadsPortfolioPath}");
Console.WriteLine($"Static files ativados: Sim");
Console.WriteLine("=========================================");

await app.RunAsync();