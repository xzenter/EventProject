using System.Text.Json;
using EventProject.Events.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EventProject.Events.Infrastructure.CachingService
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer connection, ILogger<RedisCacheService> logger)
        {
            _db = connection.GetDatabase();
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
        {
            try
            {
                var redisValue = await _db.StringGetAsync(key);

                return redisValue.HasValue
                    ? JsonSerializer.Deserialize<T>(redisValue.ToString())
                    : default;
            }
            catch (Exception ex)
            {
                // Redis недоступен - продолжаем работу без кэша
                _logger.LogWarning(ex, "Не удалось получить данные из Redis по ключу {Key}", key);

                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);

                await _db.StringSetAsync(key, json, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось сохранить данные в Redis по ключу {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken ct = default)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось удалить данные из Redis по ключу {Key}", key);
            }
        }
    }
}