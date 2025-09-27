using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace portalinmobiliario1.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache? _distributedCache;
        private readonly IMemoryCache? _memoryCache;
        private readonly bool _useRedis;

        public CacheService(IServiceProvider serviceProvider)
        {
            _distributedCache = serviceProvider.GetService<IDistributedCache>();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();
            _useRedis = _distributedCache != null;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_useRedis && _distributedCache != null)
            {
                var json = await _distributedCache.GetStringAsync(key);
                return json == null ? default : JsonSerializer.Deserialize<T>(json);
            }
            else if (_memoryCache != null)
            {
                return _memoryCache.Get<T>(key);
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            expiration ??= TimeSpan.FromSeconds(60);

            if (_useRedis && _distributedCache != null)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };
                await _distributedCache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
            }
            else if (_memoryCache != null)
            {
                _memoryCache.Set(key, value, expiration.Value);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (_useRedis && _distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key);
            }
            else if (_memoryCache != null)
            {
                _memoryCache.Remove(key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // Implementación simple para invalidar cachés relacionados
            // En producción se usaría SCAN para Redis
            if (pattern.Contains("inmuebles"))
            {
                // Invalidar cachés relacionados con inmuebles
                await RemoveAsync("inmuebles_cache");
            }
        }
    }
}
