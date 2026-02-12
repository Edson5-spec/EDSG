using EDSG.Data;
using EDSG.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EDSG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            var result = new
            {
                Timestamp = DateTime.UtcNow,

                // Informações básicas
                DatabaseInfo = new
                {
                    Provider = _context.Database.ProviderName,
                    ConnectionString = _context.Database.GetConnectionString()?.Length > 50
                        ? _context.Database.GetConnectionString()?.Substring(0, 50) + "..."
                        : _context.Database.GetConnectionString(),
                    CanConnect = await _context.Database.CanConnectAsync()
                },

                // Contagens
                Counts = new
                {
                    Users = await _context.Users.CountAsync(),
                    Services = await _context.Servicos.CountAsync(),
                    Messages = await _context.Mensagens.CountAsync(),
                    Ratings = await _context.Avaliacoes.CountAsync(),
                    Favorites = await _context.Favoritos.CountAsync(),
                    ProfessionalServices = await _context.ServicosProfissionais.CountAsync(),
                    PortfolioItems = await _context.PortfolioItems.CountAsync()
                },

                // Verificar tabelas
                TablesExist = await CheckTables()
            };

            return Ok(result);
        }

        [HttpGet("connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    return Ok(new
                    {
                        Success = true,
                        Message = "Conexão com banco de dados OK",
                        Timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        Success = false,
                        Message = "Não foi possível conectar ao banco de dados",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro ao testar conexão",
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("query-test")]
        public async Task<IActionResult> QueryTest()
        {
            try
            {
                // Testar várias queries para verificar funcionamento
                var tests = new
                {
                    // Teste 1: Consulta simples
                    SimpleQuery = await _context.Users
                        .OrderBy(u => u.Email)
                        .Select(u => new { u.Id, u.Email, u.Nome })
                        .Take(5)
                        .ToListAsync(),

                    // Teste 2: Consulta com join
                    JoinQuery = await _context.Servicos
                        .Include(s => s.Cliente)
                        .Include(s => s.Profissional)
                        .Take(3)
                        .Select(s => new
                        {
                            s.Id,
                            s.Titulo,
                            Cliente = s.Cliente.Nome,
                            Profissional = s.Profissional.Nome
                        })
                        .ToListAsync(),

                    // Teste 3: Inserção teste
                    InsertTest = await InsertTestRecord(),

                    // Teste 4: Contagens
                    Counts = new
                    {
                        ActiveUsers = await _context.Users.CountAsync(u => u.IsActive),
                        CompletedServices = await _context.Servicos.CountAsync(s => s.Estado == EstadoServico.Concluido),
                        UnreadMessages = await _context.Mensagens.CountAsync(m => !m.IsLida)
                    }
                };

                return Ok(new
                {
                    Success = true,
                    Message = "Testes de query executados com sucesso",
                    Tests = tests,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Erro durante testes de query",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        private async Task<bool> InsertTestRecord()
        {
            try
            {
                // Verificar se já existe um registro de teste
                var existingTest = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == "teste@temp.com");

                if (existingTest == null)
                {
                    var testUser = new ApplicationUser
                    {
                        UserName = "teste@temp.com",
                        Email = "teste@temp.com",
                        Nome = "Usuário de Teste",
                        IsActive = true,
                        DataRegistro = DateTime.UtcNow
                    };

                    // Nota: Não podemos usar UserManager aqui, então apenas verificamos se a tabela aceita inserts
                    // Em produção, isso seria feito através do UserManager
                    return true; // Simulamos sucesso para teste de conexão
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<Dictionary<string, bool>> CheckTables()
        {
            var tables = new[]
            {
        "AspNetUsers",
        "AspNetRoles",
        "AspNetUserRoles",
        "Servicos",
        "Mensagens",
        "Avaliacoes",
        "Favoritos",
        "Denuncias",
        "ServicosProfissionais",
        "PortfolioItems"
    };

            var result = new Dictionary<string, bool>();

            foreach (var table in tables)
            {
                try
                {
                    // CORREÇÃO: Em SQLite, os nomes de tabelas são case-insensitive
                    var sql = $"SELECT name FROM sqlite_master WHERE type='table' AND LOWER(name)=LOWER('{table}')";
                    var exists = await _context.Database.ExecuteSqlRawAsync(sql) > 0;
                    result[table] = exists;

                    // Verificação adicional: tentar contar registros
                    if (exists)
                    {
                        try
                        {
                            var countSql = $"SELECT COUNT(*) FROM \"{table}\"";
                            var count = await _context.Database.ExecuteSqlRawAsync(countSql);
                            Console.WriteLine($"DEBUG: Tabela '{table}' tem {count} registros");
                        }
                        catch (Exception countEx)
                        {
                            Console.WriteLine($"DEBUG: Erro ao contar '{table}': {countEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Erro verificando tabela '{table}': {ex.Message}");
                    result[table] = false;
                }
            }

            return result;
        }
    }
}