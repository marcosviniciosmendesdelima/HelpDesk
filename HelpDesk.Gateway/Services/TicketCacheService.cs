using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace HelpDesk.Gateway.Services
{
    public class TicketCacheService : ITicketCacheService
    {
        private readonly IDistributedCache _cache;

        public TicketCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string?> GetTicketCacheAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }

        public async Task SetTicketCacheAsync(string key, string data, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _cache.SetStringAsync(key, data, options);
        }

        public async Task RemoveTicketCacheAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }
    }
}