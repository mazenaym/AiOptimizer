//using System.Text.Json;
//using Microsoft.Extensions.Caching.Distributed;
//using PromptOptimizer.Application.Common.Interfaces;

//namespace PromptOptimizer.Infrastructure.Caching;

//public class RedisCacheService : ICacheService
//{
//    private readonly IDistributedCache _cache;

//    public RedisCacheService(IDistributedCache cache)
//    {
//        _cache = cache;
//    }

//    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
//    {
//        var data = await _cache.GetStringAsync(key, cancellationToken);
//        if (string.IsNullOrEmpty(data)) return default;

//        return JsonSerializer.Deserialize<T>(data);
//    }

//    public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
//    {
//        var options = new DistributedCacheEntryOptions
//        {
//            AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromMinutes(30)
//        };

//        var json = JsonSerializer.Serialize(value);
//        await _cache.SetStringAsync(key, json, options, cancellationToken);
//    }

//    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
//    {
//        await _cache.RemoveAsync(key, cancellationToken);
//    }
//}
