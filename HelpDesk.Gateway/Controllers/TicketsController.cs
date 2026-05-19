using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using HelpDesk.Gateway.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelpDesk.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketCacheService _cacheService;
        
        // String de conexão apontando para o container do banco na rede do Docker
        private readonly string _connectionString = "Host=helpdesk-db;Port=5432;Database=postgres;Username=postgres;Password=SenhaForte123;";
        private const string CacheKey = "tickets:all";

        public TicketsController(ITicketCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // 1. Tenta recuperar os dados do cache distribuído (Redis)
                var cachedData = await _cacheService.GetTicketCacheAsync(CacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    Console.WriteLine(" [Redis] CACHE HIT: Dados recuperados ultra-rápido do Redis!");
                    var ticketsFromCache = JsonSerializer.Deserialize<IEnumerable<object>>(cachedData);
                    return Ok(ticketsFromCache);
                }

                // 2. CACHE MISS: Se não encontrar no Redis, faz a query no PostgreSQL via Dapper
                Console.WriteLine(" [Postgres] CACHE MISS: Buscando direto na tabela TicketsRead...");
                
                using var db = new NpgsqlConnection(_connectionString);
                var sql = "SELECT id, titulo, descricao, status, prioridade FROM TicketsRead;";
                var tickets = await db.QueryAsync(sql);

                // 3. Serializa o resultado e guarda no Redis com expiração de 5 minutos (TTL)
                var jsonString = JsonSerializer.Serialize(tickets);
                await _cacheService.SetTicketCacheAsync(CacheKey, jsonString, TimeSpan.FromMinutes(5));
                Console.WriteLine(" [Redis] Cópia dos dados armazenada no cache com sucesso.");

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [Erro] Falha ao processar a listagem com cache: {ex.Message}");
                return StatusCode(500, "Erro interno ao buscar os tickets.");
            }
        }
    }
}