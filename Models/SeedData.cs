using EDSG.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EDSG.Models
{
    public static class SeedData
    {
        public static async Task Initialize(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            Console.WriteLine("=== SEED DATA INICIANDO ===");

            // Testar conexão
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"Conexão com BD: {(canConnect ? "✓ OK" : "✗ FALHOU")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO de conexão: {ex.Message}");
                return;
            }

            // ============================================
            // 1. CRIAR ROLES
            // ============================================
            Console.WriteLine("Criando roles...");

            string[] roles = { "Admin", "Professional", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✓ Role '{role}' criada");
                }
                else
                {
                    Console.WriteLine($"✓ Role '{role}' já existe");
                }
            }

            // ============================================
            // 2. CRIAR ADMIN USER
            // ============================================
            Console.WriteLine("Criando admin user...");

            var adminEmail = "admin@edsg.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador EDSG",
                    Localizacao = "Lisboa, Portugal",
                    Categoria = "Administração",
                    Especialidade = "Gestão de Sistema",
                    PrecoBase = 0,
                    Bio = "Administrador do sistema EDSG - Ecossistema Digital de Serviços Geridos",
                    IsAdmin = true,
                    IsPremium = true,
                    IsActive = true,
                    EmailConfirmed = true,
                    DataRegistro = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddToRoleAsync(adminUser, "Professional");
                    await userManager.AddToRoleAsync(adminUser, "Client");
                    Console.WriteLine("✓ Admin user criado: admin@edsg.com / Admin123!");
                }
                else
                {
                    Console.WriteLine("✗ Erro ao criar admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  - {error.Description}");
                    }
                }
            }
            else
            {
                Console.WriteLine("✓ Admin user já existe");
            }

            // ============================================
            // 3. CRIAR PROFISSIONAIS DE EXEMPLO
            // ============================================
            Console.WriteLine("Criando profissionais...");

            var professionals = new[]
            {
                new {
                    Email = "joao.silva@example.com",
                    Nome = "João Silva",
                    Localizacao = "Lisboa",
                    Categoria = "Tecnologia",
                    Especialidade = "Desenvolvimento Web",
                    PrecoBase = 35m,
                    Bio = "Desenvolvedor full-stack com 8 anos de experiência",
                    IsPremium = true
                },
                new {
                    Email = "sofia.martins@example.com",
                    Nome = "Sofia Martins",
                    Localizacao = "Porto",
                    Categoria = "Design",
                    Especialidade = "UI/UX Design",
                    PrecoBase = 45m,
                    Bio = "Designer especializada em interfaces modernas",
                    IsPremium = true
                },
                new {
                    Email = "pedro.costa@example.com",
                    Nome = "Pedro Costa",
                    Localizacao = "Braga",
                    Categoria = "Marketing",
                    Especialidade = "Marketing Digital",
                    PrecoBase = 40m,
                    Bio = "Consultor de marketing digital com foco em SEO",
                    IsPremium = false
                }
            };

            foreach (var prof in professionals)
            {
                var user = await userManager.FindByEmailAsync(prof.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = prof.Email,
                        Email = prof.Email,
                        Nome = prof.Nome,
                        Localizacao = prof.Localizacao,
                        Categoria = prof.Categoria,
                        Especialidade = prof.Especialidade,
                        PrecoBase = prof.PrecoBase,
                        Bio = prof.Bio,
                        IsPremium = prof.IsPremium,
                        IsActive = true,
                        EmailConfirmed = true,
                        DataRegistro = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, "Teste123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Professional");
                        await userManager.AddToRoleAsync(user, "Client");
                        Console.WriteLine($"✓ Profissional criado: {prof.Email}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Erro ao criar {prof.Email}");
                    }
                }
                else
                {
                    Console.WriteLine($"✓ Profissional já existe: {prof.Email}");
                }
            }

            // ============================================
            // 4. CRIAR CLIENTES DE EXEMPLO
            // ============================================
            Console.WriteLine("Criando clientes...");

            var clients = new[]
            {
                new {
                    Email = "cliente1@example.com",
                    Nome = "Maria Santos",
                    Localizacao = "Lisboa",
                    Bio = "Empresária no setor do turismo"
                },
                new {
                    Email = "cliente2@example.com",
                    Nome = "Carlos Fernandes",
                    Localizacao = "Porto",
                    Bio = "Gestor de uma startup tecnológica"
                }
            };

            foreach (var client in clients)
            {
                var user = await userManager.FindByEmailAsync(client.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = client.Email,
                        Email = client.Email,
                        Nome = client.Nome,
                        Localizacao = client.Localizacao,
                        Bio = client.Bio,
                        IsActive = true,
                        EmailConfirmed = true,
                        DataRegistro = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, "Teste123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Client");
                        Console.WriteLine($"✓ Cliente criado: {client.Email}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Erro ao criar {client.Email}");
                    }
                }
                else
                {
                    Console.WriteLine($"✓ Cliente já existe: {client.Email}");
                }
            }

            // ============================================
            // 5. CRIAR SERVIÇOS DE EXEMPLO
            // ============================================
            Console.WriteLine("Criando serviços...");

            if (!context.Servicos.Any())
            {
                var clientes = await userManager.GetUsersInRoleAsync("Client");
                var profissionais = await userManager.GetUsersInRoleAsync("Professional");

                if (clientes.Any() && profissionais.Any())
                {
                    var joao = profissionais.First(u => u.Email == "joao.silva@example.com");
                    var sofia = profissionais.First(u => u.Email == "sofia.martins@example.com");
                    var maria = clientes.First(u => u.Email == "cliente1@example.com");
                    var carlos = clientes.First(u => u.Email == "cliente2@example.com");

                    var services = new[]
                    {
                        new Servico
                        {
                            ClienteId = maria.Id,
                            ProfissionalId = joao.Id,
                            Titulo = "Desenvolvimento de Website Corporativo",
                            Descricao = "Preciso de um website moderno para minha empresa de turismo.",
                            Categoria = "Tecnologia",
                            Localizacao = "Lisboa",
                            PrecoAcordado = 2500,
                            Estado = EstadoServico.Concluido,
                            DataPedido = DateTime.UtcNow.AddDays(-30),
                            DataAceitacao = DateTime.UtcNow.AddDays(-28),
                            DataConclusao = DateTime.UtcNow.AddDays(-10)
                        },
                        new Servico
                        {
                            ClienteId = carlos.Id,
                            ProfissionalId = sofia.Id,
                            Titulo = "Redesign de Interface Mobile",
                            Descricao = "Necessito de um redesign completo do aplicativo da minha startup.",
                            Categoria = "Design",
                            Localizacao = "Porto",
                            PrecoAcordado = 1800,
                            Estado = EstadoServico.EmProgresso,
                            DataPedido = DateTime.UtcNow.AddDays(-15),
                            DataAceitacao = DateTime.UtcNow.AddDays(-14)
                        }
                    };

                    context.Servicos.AddRange(services);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✓ {services.Length} serviços criados");
                }
                else
                {
                    Console.WriteLine("✗ Não há usuários suficientes para criar serviços");
                }
            }
            else
            {
                Console.WriteLine("✓ Serviços já existem");
            }

            // ============================================
            // 6. CRIAR AVALIAÇÕES
            // ============================================
            Console.WriteLine("Criando avaliações...");

            if (!context.Avaliacoes.Any())
            {
                var servicos = context.Servicos.ToList();
                if (servicos.Any())
                {
                    var avaliacoes = new[]
                    {
                        new Avaliacao
                        {
                            ServicoId = servicos[0].Id,
                            AvaliadorId = servicos[0].ClienteId,
                            AvaliadoId = servicos[0].ProfissionalId,
                            Nota = 5,
                            Comentario = "Excelente trabalho! Entregou antes do prazo.",
                            DataAvaliacao = DateTime.UtcNow.AddDays(-9)
                        },
                        new Avaliacao
                        {
                            ServicoId = servicos[1].Id,
                            AvaliadorId = servicos[1].ClienteId,
                            AvaliadoId = servicos[1].ProfissionalId,
                            Nota = 4,
                            Comentario = "Bom trabalho, comunicação poderia melhorar.",
                            DataAvaliacao = DateTime.UtcNow.AddDays(-5)
                        }
                    };

                    context.Avaliacoes.AddRange(avaliacoes);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✓ {avaliacoes.Length} avaliações criadas");
                }
            }
            else
            {
                Console.WriteLine("✓ Avaliações já existem");
            }

            // ============================================
            // 7. CRIAR MENSAGENS
            // ============================================
            Console.WriteLine("Criando mensagens...");

            if (!context.Mensagens.Any())
            {
                var users = context.Users.Take(2).ToList();
                if (users.Count >= 2)
                {
                    var mensagens = new[]
                    {
                        new Mensagem
                        {
                            RemetenteId = users[0].Id,
                            DestinatarioId = users[1].Id,
                            Texto = "Olá! Gostei muito do seu portfólio.",
                            DataEnvio = DateTime.UtcNow.AddHours(-48),
                            IsLida = true
                        },
                        new Mensagem
                        {
                            RemetenteId = users[1].Id,
                            DestinatarioId = users[0].Id,
                            Texto = "Obrigado! Envio em anexo meu portfolio.",
                            DataEnvio = DateTime.UtcNow.AddHours(-47),
                            IsLida = true
                        }
                    };

                    context.Mensagens.AddRange(mensagens);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✓ {mensagens.Length} mensagens criadas");
                }
            }
            else
            {
                Console.WriteLine("✓ Mensagens já existem");
            }

            // ============================================
            // 8. CRIAR FAVORITOS
            // ============================================
            Console.WriteLine("Criando favoritos...");

            if (!context.Favoritos.Any())
            {
                var clientes = await userManager.GetUsersInRoleAsync("Client");
                var profissionais = await userManager.GetUsersInRoleAsync("Professional");

                if (clientes.Any() && profissionais.Any())
                {
                    var favoritos = new[]
                    {
                        new Favorito
                        {
                            ClienteId = clientes.First().Id,
                            ProfissionalId = profissionais.First().Id,
                            DataAdicao = DateTime.UtcNow.AddDays(-20)
                        }
                    };

                    context.Favoritos.AddRange(favoritos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✓ {favoritos.Length} favoritos criados");
                }
            }
            else
            {
                Console.WriteLine("✓ Favoritos já existem");
            }

            // ============================================
            // 9. CRIAR SERVIÇOS PROFISSIONAIS
            // ============================================
            Console.WriteLine("Criando serviços profissionais...");

            if (!context.ServicosProfissionais.Any())
            {
                var profissionais = await userManager.GetUsersInRoleAsync("Professional");
                if (profissionais.Any())
                {
                    var joao = profissionais.First(u => u.Email == "joao.silva@example.com");

                    var servicoProfissional = new ServicoProfissional
                    {
                        ProfissionalId = joao.Id,
                        Nome = "Desenvolvimento de E-commerce",
                        Descricao = "Criação de lojas online completas com sistema de pagamentos.",
                        Categoria = "Tecnologia",
                        Preco = 2000,
                        TempoEstimado = "2-4 semanas",
                        IsAtivo = true,
                        DataCriacao = DateTime.UtcNow
                    };

                    context.ServicosProfissionais.Add(servicoProfissional);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✓ Serviço profissional criado");
                }
            }
            else
            {
                Console.WriteLine("✓ Serviços profissionais já existem");
            }

            // ============================================
            // FINALIZAÇÃO
            // ============================================

            var totalUsers = await context.Users.CountAsync();
            var totalServices = await context.Servicos.CountAsync();
            var totalMessages = await context.Mensagens.CountAsync();

            Console.WriteLine("=== SEED DATA FINALIZADO ===");
            Console.WriteLine($"Total Usuários: {totalUsers}");
            Console.WriteLine($"Total Serviços: {totalServices}");
            Console.WriteLine($"Total Mensagens: {totalMessages}");
            Console.WriteLine("=============================");
        }
    }
}