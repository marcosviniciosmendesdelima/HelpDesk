using System;
using System.Threading.Tasks;

namespace HelpDesk.Gateway.Services
{
    public interface ITicketCacheService
    {
        Task<string?> GetTicketCacheAsync(string key);
        Task SetTicketCacheAsync(string key, string data, TimeSpan expiration);
        Task RemoveTicketCacheAsync(string key);
    }
}