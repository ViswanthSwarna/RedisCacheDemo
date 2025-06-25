using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace RedisCacheDemo
{
    public class CacheFunction
    {
        private readonly RedisService _redis;
        private readonly FakeDatabase _db;

        public CacheFunction(RedisService redis, FakeDatabase db)
        {
            _redis = redis;
            _db = db;
        }

        [Function("CacheFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "delete")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var id = query["id"] ?? "1";
            var policy = query["policy"] ?? "read-through";

            string result;

            switch (policy)
            {
                case "read-through":
                    result = await _redis.ReadThroughAsync(id, () => _db.GetAsync(id));
                    break;

                case "write-through":
                    await _redis.WriteThroughAsync(id, $"Value-{DateTime.Now}", () => _db.WriteAsync(id, $"Value-{DateTime.Now}"));
                    result = "Write-through done.";
                    break;

                case "write-behind":
                    await _redis.WriteBehindAsync(id, $"Deferred-{DateTime.Now}");
                    result = "Write-behind queued.";
                    break;

                case "evict":
                    await _redis.EvictAsync(id);
                    result = "Key evicted.";
                    break;

                case "cache-aside":
                    var cached = await _redis.ReadThroughAsync(id, () => Task.FromResult(string.Empty), TimeSpan.FromSeconds(5));
                    result = $"Cache-aside: {cached}";
                    break;

                default:
                    result = "Unknown policy.";
                    break;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(result);
            return response;
        }
    }

}
