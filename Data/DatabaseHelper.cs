using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EDSG.Data
{
    public static class DatabaseHelper
    {
        public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseHelper");
            var context = services.GetRequiredService<AppDbContext>();

            try
            {
                logger.LogInformation("=== INICIANDO INICIALIZAÇÃO DO BANCO ===");

                // 1. Garantir que o banco de dados seja criado
                logger.LogInformation("Criando/verificando banco de dados...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("✓ Banco de dados verificado/criado.");

                // 2. Testar conexão
                var canConnect = await context.Database.CanConnectAsync();
                logger.LogInformation($"✓ Conexão com banco: {(canConnect ? "OK" : "FALHA")}");

                if (!canConnect)
                {
                    logger.LogError("Não foi possível conectar ao banco de dados");
                    return;
                }

                // 3. Verificar tabelas essenciais
                var essentialTables = new[]
                {
                    "AspNetUsers",
                    "AspNetRoles",
                    "AspNetUserRoles",
                    "Servicos",
                    "Mensagens"
                };

                foreach (var table in essentialTables)
                {
                    try
                    {
                        var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{table}'";
                        var exists = await context.Database.ExecuteSqlRawAsync(sql) > 0;
                        logger.LogInformation($"Tabela '{table}': {(exists ? "✓ EXISTE" : "✗ NÃO EXISTE")}");

                        if (!exists && table.StartsWith("AspNet"))
                        {
                            logger.LogWarning($"Tabela Identity '{table}' não encontrada. O Identity pode não funcionar corretamente.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Erro ao verificar tabela {table}: {ex.Message}");
                    }
                }

                // 4. Verificar contagens básicas
                try
                {
                    var userCount = await context.Users.CountAsync();
                    logger.LogInformation($"✓ Total de usuários: {userCount}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Erro ao contar usuários: {ex.Message}");
                }

                logger.LogInformation("=== INICIALIZAÇÃO CONCLUÍDA ===");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ERRO CRÍTICO durante inicialização do banco");
                throw;
            }
        }

        public static async Task<DatabaseStatus> GetDatabaseStatus(AppDbContext context)
        {
            try
            {
                var status = new DatabaseStatus
                {
                    CanConnect = await context.Database.CanConnectAsync(),
                    ProviderName = context.Database.ProviderName,
                    ConnectionString = context.Database.GetConnectionString()
                };

                if (status.CanConnect)
                {
                    status.TableCounts = new System.Collections.Generic.Dictionary<string, int>
                    {
                        ["Users"] = await context.Users.CountAsync(),
                        ["Services"] = await context.Servicos.CountAsync(),
                        ["Messages"] = await context.Mensagens.CountAsync(),
                        ["Ratings"] = await context.Avaliacoes.CountAsync(),
                        ["Favorites"] = await context.Favoritos.CountAsync()
                    };
                }

                return status;
            }
            catch (Exception ex)
            {
                return new DatabaseStatus
                {
                    CanConnect = false,
                    Error = ex.Message
                };
            }
        }
    }

    public class DatabaseStatus
    {
        public bool CanConnect { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        public System.Collections.Generic.Dictionary<string, int> TableCounts { get; set; }
        public string Error { get; set; }
    }
}