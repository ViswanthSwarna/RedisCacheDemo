using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedisCacheDemo;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton(new RedisService("your-redis-name.redis.cache.windows.net:6380,password=YourPassword,ssl=True,abortConnect=False"));
builder.Services.AddSingleton<FakeDatabase>();
builder.Services.AddFunctionsWorkerDefaults();


builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
