using MyProject.Application.Abstractions.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace MyProject.Infrastructure.Cache
{
    internal sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
    {
        private readonly IDatabase _db = redis.GetDatabase();

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var value = await _db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            => await _db.KeyDeleteAsync(key);
    }
}
