using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisCacheDemo
{
    public class RedisService
    {
        private readonly IDatabase _cache;
        private readonly ConnectionMultiplexer _redis;
        private const string KEY_PREFIX = "product:";

        public RedisService(string redisConnection)
        {
            _redis = ConnectionMultiplexer.Connect(redisConnection);
            _cache = _redis.GetDatabase();
        }

        public async Task<string?> ReadThroughAsync(string key, Func<Task<string>> dbFetch, TimeSpan? ttl = null)
        {
            var fullKey = KEY_PREFIX + key;
            var cached = await _cache.StringGetAsync(fullKey);
            if (!cached.IsNullOrEmpty)
                return cached;

            var dbValue = await dbFetch();
            await _cache.StringSetAsync(fullKey, dbValue, ttl ?? TimeSpan.FromMinutes(5));
            return dbValue;
        }

        public async Task WriteThroughAsync(string key, string value, Func<Task> dbWrite)
        {
            await dbWrite();
            await _cache.StringSetAsync(KEY_PREFIX + key, value);
        }

        public async Task WriteBehindAsync(string key, string value)
        {
            await _cache.StringSetAsync(KEY_PREFIX + key, value);
            _ = Task.Run(async () => {
                await Task.Delay(5000); // defer
                Console.WriteLine($"[WriteBehind] Writing to DB: {value}");
            });
        }

        public async Task EvictAsync(string key)
        {
            await _cache.KeyDeleteAsync(KEY_PREFIX + key);
        }
    }

}
